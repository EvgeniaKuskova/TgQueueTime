using Domain.Entities;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using TgQueueTime.Application;
using Xunit;

namespace Domain.Services;

public class QueueServiceTest
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
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
            new Repository<ServiceEntity?>(context));

        await queueService.AddClientToQueueAsync(client,
            new Organization(organization.Id, "Test Organization", servicesProvided));

        var clientsInQueue1 = await context.Clients.Where(c => c.QueueId == queue1.Id).ToListAsync();
        var clientsInQueue2 = await context.Clients.Where(c => c.QueueId == queue2.Id).ToListAsync();

        Assert.NotEmpty(clientsInQueue1); // Клиент добавлен в первую очередь
        Assert.Empty(clientsInQueue2); // Вторая очередь пуста
    }

    /*[Fact]
    public async Task GetClientTimeQuery_Should_Calculate_Correct_Wait_Time()
    {
        var dbContext = GetDbContext();
        var queueServiceRepository = new Repository<QueueServicesEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var organizationRepository = new Repository<OrganizationEntity>(dbContext);

        var now = DateTime.UtcNow;

        var queueEntity = new QueueEntity { OrganizationId = 1, WindowNumber = 1 };
        await queueRepository.AddAsync(queueEntity);

        var serviceEntity = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:05:00",
            OrganizationId = 1
        };
        await serviceRepository.AddAsync(serviceEntity);

        var queueServiceEntity = new QueueServicesEntity
        {
            QueueId = queueEntity.Id,
            ServiceId = serviceEntity.Id
        };
        await queueServiceRepository.AddAsync(queueServiceEntity);

        await dbContext.SaveChangesAsync();

        var startTimeForFirstClient =
            now.AddMinutes(-3).ToString("o"); // Первый клиент начал обслуживание 3 минуты назад
        var clients = new List<ClientsEntity>
        {
            new ClientsEntity
            {
                QueueId = queueEntity.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 1,
                Position = 1,
                StartTime = startTimeForFirstClient
            },
            new ClientsEntity
            {
                QueueId = queueEntity.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 2,
                Position = 2,
                StartTime = null
            },
            new ClientsEntity
            {
                QueueId = queueEntity.Id,
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

        // Тестируемый клиент — второй в очереди
        var testClient = clients[1];

        var queueService = new QueueService(queueRepository, queueServiceRepository, clientRepository,
            organizationRepository, serviceRepository);

        var waitTime = await queueService.GetClientTimeQuery(testClient);

        var elapsedTimeForFirstClient = TimeSpan.FromMinutes(3);
        var serviceAverageTime = TimeSpan.FromMinutes(5);

        var remainingTimeForFirstClient = serviceAverageTime - elapsedTimeForFirstClient;

        remainingTimeForFirstClient =
            remainingTimeForFirstClient > TimeSpan.Zero ? remainingTimeForFirstClient : TimeSpan.Zero;

        var expectedWaitTime = remainingTimeForFirstClient + serviceAverageTime;

        Assert.Equal(expectedWaitTime, waitTime);
    }*/


    [Fact]
    public async Task MoveQueue_Should_Update_Queue_Correctly()
    {
        var dbContext = GetDbContext();
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var queueServicesRepository = new Repository<QueueServicesEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
        var organizationRepository = new Repository<OrganizationEntity>(dbContext);

        var queueService = new QueueService(queueRepository, queueServicesRepository, clientRepository,
            organizationRepository, serviceRepository);

        var organizationEntity = new OrganizationEntity { Name = "Test Organization" };
        await organizationRepository.AddAsync(organizationEntity);
        await dbContext.SaveChangesAsync();

        var serviceEntity = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:05:00",
            OrganizationId = organizationEntity.Id
        };
        await serviceRepository.AddAsync(serviceEntity);
        await dbContext.SaveChangesAsync();

        var queueEntity = new QueueEntity
        {
            OrganizationId = organizationEntity.Id,
            WindowNumber = 1
        };
        await queueRepository.AddAsync(queueEntity);
        await dbContext.SaveChangesAsync();

        var queueServiceEntity = new QueueServicesEntity
        {
            QueueId = queueEntity.Id,
            ServiceId = serviceEntity.Id
        };
        await queueServicesRepository.AddAsync(queueServiceEntity);
        await dbContext.SaveChangesAsync();

        var startTime = DateTime.Now.AddMinutes(-6).ToString("o"); // Клиент обслуживается уже 6 минут
        var clients = new List<ClientsEntity>
        {
            new ClientsEntity
            {
                QueueId = queueEntity.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 1,
                Position = 1,
                StartTime = startTime
            },
            new ClientsEntity
            {
                QueueId = queueEntity.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 2,
                Position = 2,
                StartTime = null
            },
            new ClientsEntity
            {
                QueueId = queueEntity.Id,
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

        var organization = new Organization(organizationEntity.Id, organizationEntity.Name);

        await queueService.MoveQueue(organization, 1);

        var remainingClients = await clientRepository
            .GetAllByValueAsync(c => c.QueueId, queueEntity.Id)
            .OrderBy(c => c.Position)
            .ToListAsync();

        Assert.Equal(2, remainingClients.Count);
        Assert.DoesNotContain(remainingClients, c => c.UserId == 1);

        var nextClient = remainingClients.FirstOrDefault(c => c.UserId == 2);
        Assert.NotNull(nextClient);
        Assert.NotNull(nextClient.StartTime); // Следующий клиент должен получить StartTime
        Assert.Equal(2, nextClient.Position); // Позиция следующего клиента остается неизменной
    }

    [Fact]
    public async Task GetNumberClientsBeforeQuery_Should_Return_Correct_Count()
    {
        var dbContext = GetDbContext();
        var clientRepository = new Repository<ClientsEntity>(dbContext);

        var queueId = 1L;

        var clients = new List<ClientsEntity>
        {
            new ClientsEntity { QueueId = queueId, UserId = 1, Position = 1, StartTime = null },
            new ClientsEntity { QueueId = queueId, UserId = 2, Position = 2, StartTime = null },
            new ClientsEntity { QueueId = queueId, UserId = 3, Position = 3, StartTime = DateTime.Now.ToString("o") },
            new ClientsEntity { QueueId = queueId, UserId = 4, Position = 4, StartTime = null }
        };

        foreach (var client in clients)
        {
            await clientRepository.AddAsync(client);
        }

        await dbContext.SaveChangesAsync();

        var testClient = clients[3];

        var queueService = new QueueService(null, null, clientRepository, null, null);

        var countBefore = await queueService.GetNumberClientsBeforeQuery(testClient);

        Assert.Equal(2, countBefore);
    }

    [Fact]
    public async Task GetAllClientsInQueueQuery_Should_Return_All_Clients_In_Queue()
    {
        // Arrange
        var dbContext = GetDbContext();
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
        var queueServiceRepository = new Repository<QueueServicesEntity>(dbContext);
        var organizationRepository = new Repository<OrganizationEntity>(dbContext);

        var queueService = new QueueService(queueRepository, queueServiceRepository, clientRepository,
            organizationRepository, serviceRepository);

        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var service = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:05:00",
            OrganizationId = organization.Id
        };
        await serviceRepository.AddAsync(service);
        await dbContext.SaveChangesAsync();

        var queue = new QueueEntity
        {
            OrganizationId = organization.Id,
            WindowNumber = 1
        };
        await queueRepository.AddAsync(queue);
        await dbContext.SaveChangesAsync();

        var queueServiceEntity = new QueueServicesEntity
        {
            QueueId = queue.Id,
            ServiceId = service.Id
        };
        await queueServiceRepository.AddAsync(queueServiceEntity);
        await dbContext.SaveChangesAsync();

        var clients = new List<ClientsEntity>
        {
            new ClientsEntity
            {
                QueueId = queue.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 1,
                Position = 1,
                StartTime = DateTime.Now.ToString("o")
            },
            new ClientsEntity
            {
                QueueId = queue.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 2,
                Position = 2,
                StartTime = null
            },
            new ClientsEntity
            {
                QueueId = queue.Id,
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

        // Act
        var result = await queueService.GetAllClientsInQueueQuery(
            new Organization(organization.Id, organization.Name), 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].Id); // Проверка, что клиенты идут в правильном порядке
        Assert.Equal(2, result[1].Id);
        Assert.Equal(3, result[2].Id);
    }


    [Fact]
    public async Task GetAllServices_Should_Return_All_Services_For_Organization()
    {
        var dbContext = GetDbContext();
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
        var organizationRepository = new Repository<OrganizationEntity>(dbContext);
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);
        var queueServiceRepository = new Repository<QueueServicesEntity>(dbContext);

        var queueService = new QueueService(queueRepository, queueServiceRepository, clientRepository,
            organizationRepository, serviceRepository);

        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var service1 = new ServiceEntity
        {
            Name = "Test Service 1",
            AverageTime = "00:15:00",
            OrganizationId = organization.Id
        };
        var service2 = new ServiceEntity
        {
            Name = "Test Service 2",
            AverageTime = "00:30:00",
            OrganizationId = organization.Id
        };
        await serviceRepository.AddAsync(service1);
        await serviceRepository.AddAsync(service2);
        await dbContext.SaveChangesAsync();

        var domainOrganization = organization.ToDomain(serviceRepository);

        var services = await queueService.GetAllServices(domainOrganization);

        Assert.NotNull(services);
        Assert.Equal(2, services.Count);

        var serviceNames = services.Select(s => s.Name).ToList();
        Assert.Contains("Test Service 1", serviceNames);
        Assert.Contains("Test Service 2", serviceNames);

        var serviceAverageTimes = services.Select(s => s.AverageTime).ToList();
        Assert.Contains(TimeSpan.FromMinutes(15), serviceAverageTimes);
        Assert.Contains(TimeSpan.FromMinutes(30), serviceAverageTimes);
    }
}