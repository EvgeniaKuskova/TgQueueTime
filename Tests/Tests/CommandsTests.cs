using Domain.Entities;
using Domain.Services;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TgQueueTime.Application;

public class CommandsTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddClientToQueueCommand_Should_Add_Client_To_Queue()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext();

        // Репозитории
        var organizationRepository = new Repository<OrganizationEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);
        var queueServiceRepository = new Repository<QueueServicesEntity>(dbContext);

        // Сервисы
        var queueService = new QueueService(
            queueRepository,
            queueServiceRepository,
            clientRepository,
            organizationRepository,
            serviceRepository);

        var organizationService = new OrganizationService(
            queueRepository,
            queueServiceRepository,
            clientRepository,
            organizationRepository,
            serviceRepository);

        // Команды
        var commands = new Commands(
            organizationService,
            queueService,
            organizationRepository,
            serviceRepository,
            queueRepository);

        // Создаем организацию
        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(organization);

        // Создаем услугу
        var service = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:30:00",
            OrganizationId = organization.Id
        };
        await serviceRepository.AddAsync(service);

        // Создаем очередь
        var queue = new QueueEntity
        {
            OrganizationId = organization.Id,
            WindowNumber = 1
        };
        await queueRepository.AddAsync(queue);

        // Связываем очередь и услугу
        var queueServiceEntity = new QueueServicesEntity
        {
            QueueId = queue.Id,
            ServiceId = service.Id
        };
        await queueServiceRepository.AddAsync(queueServiceEntity);

        // Act
        await commands.AddClientToQueueCommand(123, "Test Service", organization.Name);

        // Assert
        var clientsInQueue = await clientRepository
            .GetAllByValueAsync(c => c.QueueId, queue.Id)
            .ToListAsync();

        Assert.Single(clientsInQueue);
        var client = clientsInQueue.First();

        var organizationInDb = await organizationRepository.GetByKeyAsync(organization.Id);
        Assert.NotNull(organizationInDb);

        var serviceInDb = await serviceRepository.GetByConditionsAsync(
            s => s.Name == service.Name && s.OrganizationId == organization.Id);
        Assert.NotNull(serviceInDb);

        var queueInDb = await queueRepository.GetAllByValueAsync(q => q.OrganizationId, organization.Id).ToListAsync();
        Assert.NotEmpty(queueInDb);

        var queueServiceInDb = await queueServiceRepository
            .GetAllByCondition(qs => qs.QueueId == queue.Id && qs.ServiceId == service.Id)
            .ToListAsync();
        Assert.NotEmpty(queueServiceInDb);

        Assert.Equal(123, client.UserId);
        Assert.Equal(queue.Id, client.QueueId);
        Assert.Equal(1, client.Position); // Клиент должен быть первым в очереди
    }

    [Fact]
    public async Task UpdateServiceAverageTimeCommand_Should_Update_AverageTime()
    {
        var dbContext = GetInMemoryDbContext();

        var organizationRepository = new Repository<OrganizationEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);
        var queueServiceRepository = new Repository<QueueServicesEntity>(dbContext);

        var organizationService = new OrganizationService(
            queueRepository,
            queueServiceRepository,
            clientRepository,
            organizationRepository,
            serviceRepository);

        var commands = new Commands(organizationService, null, organizationRepository, serviceRepository,
            queueRepository);

        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(organization);

        var service = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:30:00",
            OrganizationId = organization.Id
        };
        await serviceRepository.AddAsync(service);

        var newAverageTime = TimeSpan.FromMinutes(45);

        await commands.UpdateServiceAverageTimeCommand(organization.Id, service.Name, newAverageTime);

        var updatedService = await serviceRepository.GetByConditionsAsync(
            s => s.Name == service.Name && s.OrganizationId == organization.Id);

        Assert.NotNull(updatedService);
        Assert.Equal(newAverageTime.ToString(), updatedService.AverageTime);
    }

    [Fact]
    public async Task AddService_Should_Add_Service_To_Registered_Windows()
    {
        var dbContext = GetInMemoryDbContext();

        var organizationRepository = new Repository<OrganizationEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var queueServicesRepository = new Repository<QueueServicesEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);

        var organizationService = new OrganizationService(queueRepository, queueServicesRepository, clientRepository,
            organizationRepository, serviceRepository);

        var commands = new Commands(organizationService, null, organizationRepository, serviceRepository, queueRepository);

        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(organization);

        // Создаем зарегистрированные окна (очереди)
        var registeredWindows = new List<int> { 1, 2 };
        foreach (var window in registeredWindows)
        {
            var queue = new QueueEntity
            {
                OrganizationId = organization.Id,
                WindowNumber = window
            };
            await queueRepository.AddAsync(queue);
        }

        var serviceName = "Test Service";
        var averageTime = TimeSpan.FromMinutes(30);
        var windowsToAddService = new List<int> { 1, 2 }; // Все окна зарегистрированы

        await commands.AddService(organization.Id, serviceName, averageTime, windowsToAddService);

        var servicesInDb = await serviceRepository.GetAllByValueAsync(s => s.OrganizationId, organization.Id)
            .ToListAsync();
        Assert.Single(servicesInDb);
        var serviceInDb = servicesInDb.First();
        Assert.Equal(serviceName, serviceInDb.Name);
        Assert.Equal("00:30:00", serviceInDb.AverageTime);

        foreach (var window in registeredWindows)
        {
            var queueInDb = await queueRepository.GetByConditionsAsync(q =>
                q.OrganizationId == organization.Id && q.WindowNumber == window);
            Assert.NotNull(queueInDb);

            var queueServiceInDb = await queueServicesRepository.GetByConditionsAsync(qs =>
                qs.QueueId == queueInDb.Id && qs.ServiceId == serviceInDb.Id);
            Assert.NotNull(queueServiceInDb);
        }
    }

    [Fact]
    public async Task AddService_Should_Throw_When_Window_Not_Registered()
    {
        var dbContext = GetInMemoryDbContext();

        var organizationRepository = new Repository<OrganizationEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var queueServicesRepository = new Repository<QueueServicesEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);

        var organizationService = new OrganizationService(queueRepository, queueServicesRepository, clientRepository,
            organizationRepository, serviceRepository);

        var commands = new Commands(organizationService, null, organizationRepository, serviceRepository, queueRepository);

        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(organization);

        // Создаем зарегистрированные окна (очереди)
        var registeredWindows = new List<int> { 1 };
        foreach (var window in registeredWindows)
        {
            var queue = new QueueEntity
            {
                OrganizationId = organization.Id,
                WindowNumber = window
            };
            await queueRepository.AddAsync(queue);
        }

        var serviceName = "Test Service";
        var averageTime = TimeSpan.FromMinutes(30);
        var invalidWindows = new List<int> { 2 }; // Указано окно, которого нет в базе

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            commands.AddService(organization.Id, serviceName, averageTime, invalidWindows));

        Assert.Equal("Окно 2 не зарегистрировано в организации с id " + organization.Id + ".", exception.Message);
    }
}