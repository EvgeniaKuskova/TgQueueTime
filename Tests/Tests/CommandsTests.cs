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
            queueRepository,
            clientRepository);

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
            queueRepository, clientRepository);

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
    public async Task AddService_Should_Add_Service_And_Link_To_Windows()
    {
        var dbContext = GetInMemoryDbContext();

        var organizationRepository = new Repository<OrganizationEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var queueServicesRepository = new Repository<QueueServicesEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);

        var organizationService = new OrganizationService(
            queueRepository,
            queueServicesRepository,
            clientRepository,
            organizationRepository,
            serviceRepository);

        var queueService = new QueueService(
            queueRepository,
            queueServicesRepository,
            clientRepository,
            organizationRepository,
            serviceRepository);

        var commands = new Commands(
            organizationService,
            queueService,
            organizationRepository,
            serviceRepository,
            queueRepository,
            clientRepository);

        var organization = new OrganizationEntity
        {
            Id = 1,
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(organization);

        var windowNumbers = new List<int> { 1, 2, 3 };

        foreach (var windowNumber in windowNumbers)
        {
            var queueEntity = new QueueEntity
            {
                OrganizationId = organization.Id,
                WindowNumber = windowNumber
            };
            await queueRepository.AddAsync(queueEntity);
        }

        await dbContext.SaveChangesAsync();

        var serviceName = "Test Service";
        var averageTime = TimeSpan.FromMinutes(15);

        await commands.AddService(organization.Id, serviceName, averageTime, windowNumbers);

        var addedService = await serviceRepository
            .GetByConditionsAsync(s => s.OrganizationId == organization.Id && s.Name == serviceName);

        Assert.NotNull(addedService);

        foreach (var windowNumber in windowNumbers)
        {
            var queueEntity = await queueRepository
                .GetByConditionsAsync(q => q.OrganizationId == organization.Id && q.WindowNumber == windowNumber);

            Assert.NotNull(queueEntity);
        }
    }


    [Fact]
    public async Task AddService_Should_Throw_Exception_When_Organization_Not_Found()
    {
        var dbContext = GetInMemoryDbContext();

        var organizationRepository = new Repository<OrganizationEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var queueServicesRepository = new Repository<QueueServicesEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);

        var organizationService = new OrganizationService(
            queueRepository,
            queueServicesRepository,
            clientRepository,
            organizationRepository,
            serviceRepository);

        var queueService = new QueueService(
            queueRepository,
            queueServicesRepository,
            clientRepository,
            organizationRepository,
            serviceRepository);

        var commands = new Commands(
            organizationService,
            queueService,
            organizationRepository,
            serviceRepository,
            queueRepository,
            clientRepository);

        var nonExistentOrganizationId = 999;
        var serviceName = "Test Service";
        var averageTime = TimeSpan.FromMinutes(15);
        var windowNumbers = new List<int> { 1, 2, 3 };

        var result = await commands.AddService(nonExistentOrganizationId, 
            serviceName, averageTime, windowNumbers);

        Assert.Equal("Ваша организация не зарегистрирована.", result.Error);
    }
}