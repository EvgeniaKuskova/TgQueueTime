using Domain.Entities;
using Domain.Services;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TgQueueTime.Application;

public class CommandsTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<ServiceEntity?> _serviceRepository;
    private readonly IRepository<QueueEntity> _queueRepository;
    private readonly IRepository<QueueServicesEntity> _queueServicesRepository;
    private readonly IRepository<ClientsEntity> _clientRepository;

    private readonly OrganizationService _organizationService;
    private readonly QueueService _queueService;
    private readonly Commands _commands;

    public CommandsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _organizationRepository = new Repository<OrganizationEntity>(_dbContext);
        _serviceRepository = new Repository<ServiceEntity?>(_dbContext);
        _queueRepository = new Repository<QueueEntity>(_dbContext);
        _queueServicesRepository = new Repository<QueueServicesEntity>(_dbContext);
        _clientRepository = new Repository<ClientsEntity>(_dbContext);

        _organizationService = new OrganizationService(
            _queueRepository, _queueServicesRepository, _clientRepository, _organizationRepository, _serviceRepository);

        _queueService = new QueueService(
            _queueRepository, _queueServicesRepository, _clientRepository, _organizationRepository, _serviceRepository);

        _commands = new Commands(
            _organizationService, _queueService, _organizationRepository, _serviceRepository, _queueRepository,
            _clientRepository);
    }


    [Fact]
    public async Task AddClientToQueueCommand_Should_Add_Client_To_Queue()
    {
        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await _organizationRepository.AddAsync(organization);

        var service = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:30:00",
            OrganizationId = organization.Id
        };
        await _serviceRepository.AddAsync(service);

        var queue = new QueueEntity
        {
            OrganizationId = organization.Id,
            WindowNumber = 1
        };
        await _queueRepository.AddAsync(queue);

        var queueServiceEntity = new QueueServicesEntity
        {
            QueueId = queue.Id,
            ServiceId = service.Id
        };
        await _queueServicesRepository.AddAsync(queueServiceEntity);

        await _commands.AddClientToQueueCommand(123, "Test Service", organization.Name);

        var clientsInQueue = await _clientRepository
            .GetAllByValueAsync(c => c.QueueId, queue.Id)
            .ToListAsync();

        Assert.Single(clientsInQueue);
        var client = clientsInQueue.First();

        var organizationInDb = await _organizationRepository.GetByKeyAsync(organization.Id);
        Assert.NotNull(organizationInDb);

        var serviceInDb = await _serviceRepository.GetByConditionsAsync(
            s => s.Name == service.Name && s.OrganizationId == organization.Id);
        Assert.NotNull(serviceInDb);

        var queueInDb = await _queueRepository.GetAllByValueAsync(q => q.OrganizationId, organization.Id).ToListAsync();
        Assert.NotEmpty(queueInDb);

        var queueServiceInDb = await _queueServicesRepository
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
        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await _organizationRepository.AddAsync(organization);

        var service = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:30:00",
            OrganizationId = organization.Id
        };
        await _serviceRepository.AddAsync(service);

        var newAverageTime = TimeSpan.FromMinutes(45);

        await _commands.UpdateServiceAverageTimeCommand(organization.Id, service.Name, newAverageTime);

        var updatedService = await _serviceRepository.GetByConditionsAsync(
            s => s.Name == service.Name && s.OrganizationId == organization.Id);

        Assert.NotNull(updatedService);
        Assert.Equal(newAverageTime.ToString(), updatedService.AverageTime);
    }

    [Fact]
    public async Task AddService_Should_Add_Service_And_Link_To_Windows()
    {
        var organization = new OrganizationEntity
        {
            Id = 1,
            Name = "Test Organization"
        };
        await _organizationRepository.AddAsync(organization);

        var windowNumbers = new List<int> { 1, 2, 3 };

        foreach (var windowNumber in windowNumbers)
        {
            var queueEntity = new QueueEntity
            {
                OrganizationId = organization.Id,
                WindowNumber = windowNumber
            };
            await _queueRepository.AddAsync(queueEntity);
        }

        await _dbContext.SaveChangesAsync();

        var serviceName = "Test Service";
        var averageTime = TimeSpan.FromMinutes(15);

        await _commands.AddService(organization.Id, serviceName, averageTime, windowNumbers);

        var addedService = await _serviceRepository
            .GetByConditionsAsync(s => s.OrganizationId == organization.Id && s.Name == serviceName);

        Assert.NotNull(addedService);

        foreach (var windowNumber in windowNumbers)
        {
            var queueEntity = await _queueRepository
                .GetByConditionsAsync(q => q.OrganizationId == organization.Id && q.WindowNumber == windowNumber);

            Assert.NotNull(queueEntity);
        }
    }


    [Fact]
    public async Task AddService_Should_Throw_Exception_When_Organization_Not_Found()
    {
        var nonExistentOrganizationId = 999;
        var serviceName = "Test Service";
        var averageTime = TimeSpan.FromMinutes(15);
        var windowNumbers = new List<int> { 1, 2, 3 };

        var result = await _commands.AddService(nonExistentOrganizationId,
            serviceName, averageTime, windowNumbers);

        Assert.Equal("Ваша организация не зарегистрирована.", result.Error);
    }

    [Fact]
    public async Task RegisterOrganizationCommand_Should_Register_New_Organization()
    {
        var organizationId = 1L;
        var organizationName = "New Organization";

        var result = await _commands.RegisterOrganizationCommand(organizationId, organizationName);

        Assert.True(result.IsSuccess);

        var organizationInDb = await _organizationRepository.GetByKeyAsync(organizationId);
        Assert.NotNull(organizationInDb);
        Assert.Equal(organizationName, organizationInDb.Name);
    }

    [Fact]
    public async Task RegisterOrganizationCommand_Should_Return_Failure_When_Organization_Already_Exists()
    {
        var organization = new OrganizationEntity
        {
            Id = 1L,
            Name = "Existing Organization"
        };

        await _organizationRepository.AddAsync(organization);
        await _dbContext.SaveChangesAsync();

        var result = await _commands.RegisterOrganizationCommand(organization.Id, organization.Name);

        Assert.False(result.IsSuccess);
        Assert.Equal("Организация уже зарегистрирована на этом аккаунте", result.Error);
    }
}