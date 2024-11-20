using Domain.Entities;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Domain.Services;

public class QueueServiceTest
{
    private ApplicationDbContext GetDbContext()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var dbPath = Path.Combine(basePath, "..", "..", "..", "Infrastructure", "Database", "Database.db");
        dbPath = Path.GetFullPath(dbPath); // Получает абсолютный путь
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            //.UseSqlite($"Data Source={dbPath};")
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddDataToDatabase_Should_SaveDataCorrectly()
    {
        var context = GetDbContext();

        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };

        var service = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:30:00"
        };

        var queue = new QueueEntity
        {
            WindowNumber = 1
        };

        await context.Organizations.AddAsync(organization);
        await context.SaveChangesAsync();

        service.OrganizationId = organization.Id;
        await context.Services.AddAsync(service);
        await context.SaveChangesAsync();

        queue.OrganizationId = organization.Id;
        await context.Queues.AddAsync(queue);
        await context.SaveChangesAsync();

        var queueService = new QueueServicesEntity
        {
            QueueId = queue.Id,
            ServiceId = service.Id
        };

        await context.QueueServices.AddAsync(queueService);
        await context.SaveChangesAsync();

        var client = new ClientsEntity
        {
            QueueId = queue.Id,
            UserId = 123,
            Position = 1,
            StartTime = DateTime.Now.ToString("o"),
            QueueServiceId = queueService.Id
        };

        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();

        var organizationInDb = await context.Organizations
            .FirstOrDefaultAsync(o => o.Name == organization.Name && o.Id == organization.Id);
        var serviceInDb = await context.Services
            .FirstOrDefaultAsync(s => s.Name == service.Name && s.OrganizationId == organization.Id);
        var queueInDb = await context.Queues
            .FirstOrDefaultAsync(q => q.WindowNumber == queue.WindowNumber && q.OrganizationId == organization.Id);
    }


    [Fact]
    public async Task AddClientToQueueAsync_Should_Add_Client_To_Optimal_Queue()
    {
        var context = GetDbContext();

        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };

        await context.Organizations.AddAsync(organization);
        await context.SaveChangesAsync(); 

        var service1 = new ServiceEntity
        {
            Name = "Test Service 1",
            AverageTime = "00:30:00",
            OrganizationId = organization.Id
        };

        var service2 = new ServiceEntity
        {
            Name = "Test Service 2",
            AverageTime = "00:20:00",
            OrganizationId = organization.Id
        };

        await context.Services.AddRangeAsync(service1, service2);
        await context.SaveChangesAsync();

        var queue1 = new QueueEntity
        {
            OrganizationId = organization.Id,
            WindowNumber = 1
        };

        var queue2 = new QueueEntity
        {
            OrganizationId = organization.Id,
            WindowNumber = 2
        };

        await context.Queues.AddRangeAsync(queue1, queue2);
        await context.SaveChangesAsync();

        var queueService1 = new QueueServicesEntity
        {
            QueueId = queue1.Id, 
            ServiceId = service1.Id 
        };

        var queueService2 = new QueueServicesEntity
        {
            QueueId = queue2.Id,
            ServiceId = service2.Id
        };

        await context.QueueServices.AddRangeAsync(queueService1, queueService2);
        await context.SaveChangesAsync();

        var client = new Client(123, new Service("Test Service 1", TimeSpan.FromMinutes(30)));

        var servicesProvided = await context.Services
            .Where(s => s.OrganizationId == organization.Id)
            .Select(s => new Service(s.Name, TimeSpan.Parse(s.AverageTime)))
            .ToListAsync();

        var queueService = new QueueService(
            new Repository<QueueEntity>(context),
            new Repository<QueueServicesEntity>(context),
            new Repository<ClientsEntity>(context),
            new Repository<OrganizationEntity>(context),
            new Repository<ServiceEntity>(context));
        
        await queueService.AddClientToQueueAsync(client,
            new Organization(organization.Id, "Test Organization", servicesProvided));

        var clientsInQueue1 = await context.Clients.Where(c => c.QueueId == queue1.Id).ToListAsync();
        var clientsInQueue2 = await context.Clients.Where(c => c.QueueId == queue2.Id).ToListAsync();

        Assert.NotEmpty(clientsInQueue1); // Клиент добавлен в первую очередь
        Assert.Empty(clientsInQueue2); // Вторая очередь пуста
    }

    [Fact]
    public async Task GetClientTimeQuery_Should_Calculate_Correct_Wait_Time()
    {
        var dbContext = GetDbContext();
        var queueServiceRepository = new Repository<QueueServicesEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity>(dbContext);
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var organizationRepository = new Repository<OrganizationEntity>(dbContext);

        var queueId = 1L;

        var serviceEntity = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:05:00"
        };
        await serviceRepository.AddAsync(serviceEntity);
        await dbContext.SaveChangesAsync();

        var queueServiceEntity = new QueueServicesEntity
        {
            QueueId = queueId,
            ServiceId = serviceEntity.Id
        };
        await queueServiceRepository.AddAsync(queueServiceEntity);
        await dbContext.SaveChangesAsync();

        var lastClientStartTime = DateTime.Now.AddMinutes(-3).ToString("o");

        var clients = new List<ClientsEntity>
        {
            new ClientsEntity
            {
                QueueId = queueId,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 1,
                Position = 1,
                StartTime = lastClientStartTime
            },
            new ClientsEntity
            {
                QueueId = queueId,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 2,
                Position = 2,
                StartTime = null
            },
            new ClientsEntity
            {
                QueueId = queueId,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 3,
                Position = 3,
                StartTime = null
            }
        };

        foreach (var client in clients)
        {
            await clientRepository.AddAsync(client);
        }

        await dbContext.SaveChangesAsync();

        // Тестируемый клиент
        var testClient = clients[1];

        var queueService = new QueueService(queueRepository, queueServiceRepository, clientRepository,
            organizationRepository, serviceRepository);
        var waitTime = await queueService.GetClientTimeQuery(testClient);

        // Assert
        // Рассчитываем вручную ожидаемое время
        var elapsedTimeForLastClient = TimeSpan.FromMinutes(3); // Last client already served for 3 minutes
        var remainingTimeForLastClient = TimeSpan.FromMinutes(5) - elapsedTimeForLastClient;
        var expectedWaitTime = (remainingTimeForLastClient > TimeSpan.Zero ? remainingTimeForLastClient : TimeSpan.Zero)
                               + TimeSpan.FromMinutes(5); // Add 5 minutes for 2nd client

        // Округляем до ближайшей секунды
        var roundedWaitTime = TimeSpan.FromSeconds(Math.Round(waitTime.TotalSeconds));
        var roundedExpectedWaitTime = TimeSpan.FromSeconds(Math.Round(expectedWaitTime.TotalSeconds));

        Assert.Equal(roundedExpectedWaitTime, roundedWaitTime);
    }
}