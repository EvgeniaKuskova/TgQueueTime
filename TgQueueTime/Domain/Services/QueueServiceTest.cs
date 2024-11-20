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
            //.UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseSqlite($"Data Source={dbPath};")
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddDataToDatabase_Should_SaveDataCorrectly()
    {
        // Arrange
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

        // Act
        await context.Organizations.AddAsync(organization);
        await context.SaveChangesAsync();

        service.OrganizationId = organization.Id; // Связываем услугу с организацией
        await context.Services.AddAsync(service);
        await context.SaveChangesAsync();

        queue.OrganizationId = organization.Id; // Связываем очередь с организацией
        await context.Queues.AddAsync(queue);
        await context.SaveChangesAsync();

        var queueService = new QueueServicesEntity
        {
            QueueId = queue.Id, // Связываем очередь с услугой
            ServiceId = service.Id
        };

        await context.QueueServices.AddAsync(queueService);
        await context.SaveChangesAsync();

        var client = new ClientsEntity
        {
            QueueId = queue.Id, // Связываем клиента с очередью
            UserId = 123,
            Position = 1,
            StartTime = DateTime.Now.ToString("o"),
            QueueServiceId = queueService.Id // Связываем клиента с QueueService
        };

        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();

        // Assert
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
        // Arrange
        var context = GetDbContext();

        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };

        await context.Organizations.AddAsync(organization);
        await context.SaveChangesAsync(); // Сохраняем, чтобы получить Id

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
            QueueId = queue1.Id, // Используем существующий Id
            ServiceId = service1.Id // Используем существующий Id
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

        // Act
        await queueService.AddClientToQueueAsync(client,
            new Organization(organization.Id, "Test Organization", servicesProvided));

        // Assert
        var clientsInQueue1 = await context.Clients.Where(c => c.QueueId == queue1.Id).ToListAsync();
        var clientsInQueue2 = await context.Clients.Where(c => c.QueueId == queue2.Id).ToListAsync();

        Assert.NotEmpty(clientsInQueue1); // Клиент добавлен в первую очередь
        Assert.Empty(clientsInQueue2); // Вторая очередь пуста
    }
}