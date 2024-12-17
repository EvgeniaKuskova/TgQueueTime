using Domain.Entities;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using TgQueueTime.Application;
using Xunit;

namespace Domain.Services;

public class OrganizationServiceTest
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<QueueEntity> _queueRepository;
    private readonly IRepository<QueueServicesEntity> _queueServicesRepository;
    private readonly IRepository<ClientsEntity> _clientRepository;
    private readonly IRepository<ServiceEntity?> _serviceRepository;
    private readonly OrganizationService _organizationService;

    public OrganizationServiceTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _organizationRepository = new Repository<OrganizationEntity>(_context);
        _queueRepository = new Repository<QueueEntity>(_context);
        _queueServicesRepository = new Repository<QueueServicesEntity>(_context);
        _clientRepository = new Repository<ClientsEntity>(_context);
        _serviceRepository = new Repository<ServiceEntity?>(_context);

        _organizationService = new OrganizationService(
            _queueRepository,
            _queueServicesRepository,
            _clientRepository,
            _organizationRepository,
            _serviceRepository
        );
    }

    [Fact]
    public async Task RegisterOrganizationAsync_Should_Add_Organization_To_Database()
    {
        var organization = new Organization(1, "Test Organization meaw");

        await _organizationService.RegisterOrganizationAsync(organization);

        var organizationInDb =
            await _context.Organizations.SingleOrDefaultAsync(o => o.Name == "Test Organization meaw");
        Assert.NotNull(organizationInDb);
        Assert.Equal("Test Organization meaw", organizationInDb.Name);
    }

    [Fact]
    public async Task UpdateServiceAverageTimeCommandAsunc_Should_Update_AverageTime()
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

        var domainOrganization = new Organization(organization.Id, organization.Name);
        var domainService = new Service(service.Name, TimeSpan.Parse(service.AverageTime));

        // Новый AverageTime
        var newAverageTime = TimeSpan.FromMinutes(45);

        await _organizationService.UpdateServiceAverageTimeCommandAsunc(domainOrganization, domainService,
            newAverageTime);

        var updatedService = await _serviceRepository.GetByConditionsAsync(
            s => s.Name == service.Name && s.OrganizationId == organization.Id);

        Assert.NotNull(updatedService);
        Assert.Equal(newAverageTime.ToString(), updatedService.AverageTime);
    }

    [Fact]
    public async Task AddServiceAsync_Should_Add_Service_And_Link_To_Queue()
    {
        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await _organizationRepository.AddAsync(organization);
        await _context.SaveChangesAsync();

        var domainOrganization = new Organization(organization.Id, organization.Name);
        var serviceName = "Test Service";
        var averageTime = TimeSpan.FromMinutes(30);
        var windowNumbers = new List<int> { 1, 2, 3 };

        foreach (var windowNumber in windowNumbers)
        {
            var service = new Service(serviceName, averageTime);
            await _organizationService.AddServiceAsync(domainOrganization, service, windowNumber);
        }

        var services = _serviceRepository.GetAllByCondition(s => s.OrganizationId == organization.Id).ToList();
        Assert.NotEmpty(services);
        Assert.Contains(services, s => s.Name == serviceName);

        foreach (var windowNumber in windowNumbers)
        {
            var queueServiceLinks = _queueServicesRepository
                .GetAllByCondition(qs => qs.ServiceId == services.First().Id && qs.QueueId == windowNumber)
                .ToList();

            Assert.NotEmpty(queueServiceLinks);
            Assert.Contains(queueServiceLinks, qs => qs.QueueId == windowNumber);
        }
    }


    [Fact]
    public async Task GetAllOrganizations_Should_Return_All_Organizations()
    {
        var organizations = new List<OrganizationEntity>
        {
            new OrganizationEntity { Name = "Org 1" },
            new OrganizationEntity { Name = "Org 2" },
            new OrganizationEntity { Name = "Org 3" }
        };

        foreach (var organization in organizations)
        {
            await _organizationRepository.AddAsync(organization);
        }

        await _context.SaveChangesAsync();

        var result = await _organizationService.GetAllOrganizations();

        Assert.NotNull(result);
        Assert.Equal(organizations.Count, result.Count);
        Assert.Contains(result, o => o.Name == "Org 1");
        Assert.Contains(result, o => o.Name == "Org 2");
        Assert.Contains(result, o => o.Name == "Org 3");
    }

    [Fact]
    public async Task UpdateServiceAverageTimeCommandAsunc_Should_Throw_When_Service_Not_Found()
    {
        var domainOrganization = new Organization(1, "Nonexistent Organization");
        var domainService = new Service("Nonexistent Service", TimeSpan.FromMinutes(30));

        await Assert.ThrowsAsync<NullReferenceException>(async () =>
            await _organizationService.UpdateServiceAverageTimeCommandAsunc(domainOrganization, domainService,
                TimeSpan.FromMinutes(45)));
    }

    [Fact]
    public async Task AddServiceAsync_Should_Not_Add_Duplicate_Service()
    {
        var organization = new OrganizationEntity { Name = "Test Organization" };
        await _organizationRepository.AddAsync(organization);
        await _context.SaveChangesAsync();

        var domainOrganization = new Organization(organization.Id, organization.Name);
        var service = new Service("Test Service", TimeSpan.FromMinutes(30));

        await _organizationService.AddServiceAsync(domainOrganization, service, 1);
        await _organizationService.AddServiceAsync(domainOrganization, service, 1);

        var services = _serviceRepository.GetAllByCondition(s => s.OrganizationId == organization.Id).ToList();
        Assert.Single(services);
    }

    [Fact]
    public async Task AddServiceAsync_Should_Throw_When_Organization_Not_Found()
    {
        var nonexistentOrganization = new Organization(999, "Nonexistent Org");
        var service = new Service("Test Service", TimeSpan.FromMinutes(30));

        var exception = await Record.ExceptionAsync(async () =>
            await _organizationService.AddServiceAsync(nonexistentOrganization, service, 1));
        Assert.Null(exception);
    }
}