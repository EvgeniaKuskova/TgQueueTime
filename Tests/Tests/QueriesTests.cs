namespace TgQueueTime.Application;

using Domain.Entities;
using Domain.Services;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class QueriesTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<ServiceEntity?> _serviceRepository;
    private readonly IRepository<QueueEntity> _queueRepository;
    private readonly IRepository<ClientsEntity> _clientRepository;
    private readonly IRepository<QueueServicesEntity> _queueServicesRepository;

    private readonly OrganizationService _organizationService;
    private readonly QueueService _queueService;
    private readonly Queries _queries;

    public QueriesTests()
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
        _queries = new Queries(
            _organizationService, _queueService, _organizationRepository, _serviceRepository,
            _queueRepository, _clientRepository, _queueServicesRepository);
    }

    [Fact]
    public async Task GetClientTimeQuery_Should_Return_Correct_Time()
    {
        var organization = new OrganizationEntity
        {
            Id = 1,
            Name = "Test Organization"
        };
        await _organizationRepository.AddAsync(organization);

        var service = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:10:00",
            OrganizationId = organization.Id
        };
        await _serviceRepository.AddAsync(service);

        var queue = new QueueEntity
        {
            OrganizationId = organization.Id,
            WindowNumber = 1
        };
        await _queueRepository.AddAsync(queue);

        var queueService = new QueueServicesEntity
        {
            QueueId = queue.Id,
            ServiceId = service.Id
        };
        await _queueServicesRepository.AddAsync(queueService);

        var client = new ClientsEntity
        {
            UserId = 123,
            QueueId = queue.Id,
            QueueServiceId = queueService.Id,
            Position = 1,
            StartTime = null
        };
        await _clientRepository.AddAsync(client);

        await _dbContext.SaveChangesAsync();

        var result = await _queries.GetClientTimeQuery(client.UserId);

        Assert.True(result.IsSuccess);
        Assert.Equal(TimeSpan.FromMinutes(0), result.Value); // очередь не запущена
    }


    [Fact]
    public async Task GetClientTimeQuery_Should_Fail_When_Client_Not_Found()
    {
        var result = await _queries.GetClientTimeQuery(999);

        Assert.False(result.IsSuccess);
        Assert.Equal("Клиент с id 999 не стоит в очереди", result.Error);
    }

    [Fact]
    public async Task GetNumberClientsBeforeQuery_Should_Return_Correct_Count()
    {
        var client1 = new ClientsEntity { UserId = 1, QueueId = 1, Position = 1, StartTime = null };
        var client2 = new ClientsEntity { UserId = 2, QueueId = 1, Position = 2, StartTime = null };
        var client3 = new ClientsEntity { UserId = 3, QueueId = 1, Position = 3, StartTime = null };

        await _clientRepository.AddAsync(client1);
        await _clientRepository.AddAsync(client2);
        await _clientRepository.AddAsync(client3);
        await _dbContext.SaveChangesAsync();

        var result = await _queries.GetNumberClientsBeforeQuery(3);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task GetNumberClientsBeforeQuery_Should_Fail_When_Client_Not_Found()
    {
        var result = await _queries.GetNumberClientsBeforeQuery(999);
        Assert.False(result.IsSuccess);
        Assert.Equal("Клиент с id 999 не стоит в очереди", result.Error);
    }

    [Fact]
    public async Task GetAllClientsInQueueQuery_Should_Fail_When_Organization_Not_Found()
    {
        var result = await _queries.GetAllClientsInQueueQuery(999, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal("Организация с id 999 не найдена.", result.Error);
    }

    [Fact]
    public async Task GetAllServices_Should_Return_Services()
    {
        var organization = new OrganizationEntity { Id = 1, Name = "Test Organization" };
        await _organizationRepository.AddAsync(organization);

        var service1 = new ServiceEntity { Name = "Service 1", AverageTime = "00:15:00", OrganizationId = 1 };
        var service2 = new ServiceEntity { Name = "Service 2", AverageTime = "00:30:00", OrganizationId = 1 };

        await _serviceRepository.AddAsync(service1);
        await _serviceRepository.AddAsync(service2);
        await _dbContext.SaveChangesAsync();

        var result = await _queries.GetAllServices("Test Organization");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetAllServices_Should_Fail_When_Organization_Not_Found()
    {
        var result = await _queries.GetAllServices("Nonexistent Organization");

        Assert.False(result.IsSuccess);
        Assert.Equal("Организация с именем {nameOrganization} не найдена.", result.Error);
    }

    [Fact]
    public async Task GetAllOrganizations_Should_Return_All_Organizations()
    {
        var organization1 = new OrganizationEntity { Name = "Org 1" };
        var organization2 = new OrganizationEntity { Name = "Org 2" };

        await _organizationRepository.AddAsync(organization1);
        await _organizationRepository.AddAsync(organization2);
        await _dbContext.SaveChangesAsync();

        var result = await _queries.GetAllOrganizations();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
}