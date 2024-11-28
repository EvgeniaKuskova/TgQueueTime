using Domain.Entities;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using TgQueueTime.Application;
using Xunit;

namespace Domain.Services;

public class OrganizationServiceTest
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
    public async Task RegisterOrganizationAsync_Should_Add_Organization_To_Database()
    {
        var context = GetDbContext();
        var organizationRepository = new Repository<OrganizationEntity>(context);
        var queueRepository = new Repository<QueueEntity>(context);
        var queueServicesRepository = new Repository<QueueServicesEntity>(context);
        var clientRepository = new Repository<ClientsEntity>(context);
        var serviceRepository = new Repository<ServiceEntity?>(context);

        var organizationService = new OrganizationService(
            queueRepository,
            queueServicesRepository,
            clientRepository,
            organizationRepository,
            serviceRepository
        );

        var organization = new Organization(1, "Test Organization meaw");

        await organizationService.RegisterOrganizationAsync(organization);

        var organizationInDb =
            await context.Organizations.SingleOrDefaultAsync(o => o.Name == "Test Organization meaw");
        Assert.NotNull(organizationInDb);
        Assert.Equal("Test Organization meaw", organizationInDb.Name);
    }

    [Fact]
    public async Task UpdateServiceAverageTimeCommandAsunc_Should_Update_AverageTime()
    {
        var dbContext = GetDbContext();

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

        // Создаем доменные модели для теста
        var domainOrganization = new Organization(organization.Id, organization.Name);
        var domainService = new Service(service.Name, TimeSpan.Parse(service.AverageTime));

        // Новый AverageTime
        var newAverageTime = TimeSpan.FromMinutes(45);

        await organizationService.UpdateServiceAverageTimeCommandAsunc(domainOrganization, domainService,
            newAverageTime);

        var updatedService = await serviceRepository.GetByConditionsAsync(
            s => s.Name == service.Name && s.OrganizationId == organization.Id);

        Assert.NotNull(updatedService);
        Assert.Equal(newAverageTime.ToString(), updatedService.AverageTime);
    }

    [Fact]
    public async Task AddServiceAsync_Should_Add_Service_And_Link_To_Queue()
    {
        var dbContext = GetDbContext();
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
        var queueRepository = new Repository<QueueEntity>(dbContext);
        var queueServicesRepository = new Repository<QueueServicesEntity>(dbContext);
        var organizationRepository = new Repository<OrganizationEntity>(dbContext);

        var organizationService = new OrganizationService(
            queueRepository, queueServicesRepository, null, organizationRepository, serviceRepository);

        var organizationEntity = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(organizationEntity);

        var queueEntity = new QueueEntity
        {
            OrganizationId = organizationEntity.Id,
            WindowNumber = 1
        };
        await queueRepository.AddAsync(queueEntity);

        var organization = new Organization(organizationEntity.Id, organizationEntity.Name);
        var service = new Service("Test Service", TimeSpan.FromMinutes(30));

        await organizationService.AddServiceAsync(organization, service, 1);

        var serviceInDb = await serviceRepository.GetByConditionsAsync(s =>
            s.Name == service.Name && s.OrganizationId == organization.Id);
        Assert.NotNull(serviceInDb);
        Assert.Equal(service.Name, serviceInDb.Name);
        Assert.Equal("00:30:00", serviceInDb.AverageTime);
        var queueInDb = await queueRepository.GetByConditionsAsync(q =>
            q.OrganizationId == organization.Id && q.WindowNumber == 1);
        Assert.NotNull(queueInDb);

        var queueServiceInDb = await queueServicesRepository.GetByConditionsAsync(qs =>
            qs.QueueId == queueInDb.Id && qs.ServiceId == serviceInDb.Id);
        Assert.NotNull(queueServiceInDb);
        Assert.Equal(queueInDb.Id, queueServiceInDb.QueueId);
        Assert.Equal(serviceInDb.Id, queueServiceInDb.ServiceId);

        await organizationService.AddServiceAsync(organization, service, 1);

        var allQueueServices = await queueServicesRepository
            .GetAllByCondition(qs => qs.QueueId == queueInDb.Id && qs.ServiceId == serviceInDb.Id)
            .ToListAsync();
        Assert.Single(allQueueServices);
    }
    
    [Fact]
    public async Task GetAllOrganizations_Should_Return_All_Organizations()
    {
        var dbContext = GetDbContext();
        var organizationRepository = new Repository<OrganizationEntity>(dbContext);
        var serviceRepository = new Repository<ServiceEntity?>(dbContext);
    
        var organizationService = new OrganizationService(
            new Repository<QueueEntity>(dbContext),
            new Repository<QueueServicesEntity>(dbContext),
            new Repository<ClientsEntity>(dbContext),
            organizationRepository,
            serviceRepository
        );
    
        var organizations = new List<OrganizationEntity>
        {
            new OrganizationEntity { Name = "Org 1" },
            new OrganizationEntity { Name = "Org 2" },
            new OrganizationEntity { Name = "Org 3" }
        };
    
        foreach (var organization in organizations)
        {
            await organizationRepository.AddAsync(organization);
        }
    
        await dbContext.SaveChangesAsync();
    
        var result = await organizationService.GetAllOrganizations();
    
        Assert.NotNull(result);
        Assert.Equal(organizations.Count, result.Count);
        Assert.Contains(result, o => o.Name == "Org 1");
        Assert.Contains(result, o => o.Name == "Org 2");
        Assert.Contains(result, o => o.Name == "Org 3");
    }

}