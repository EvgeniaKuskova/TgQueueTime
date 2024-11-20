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
        var organizationRepository = new Repository<OrganizationEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity>(dbContext);
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var clientRepository = new Repository<ClientsEntity>(dbContext);
        var queueServiceRepository = new Repository<QueueServicesEntity>(dbContext);

        var queueService = new QueueService(queueRepository, queueServiceRepository, clientRepository,
            organizationRepository, serviceRepository);
        var organizationService = new OrganizationService(organizationRepository);

        var commands = new Commands(organizationService, queueService, organizationRepository, serviceRepository);

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
            
        var organizationInDb = await organizationRepository.GetByIdAsync(organization.Id);
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
}