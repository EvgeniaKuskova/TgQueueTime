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
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
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
            serviceRepository
        );

        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var domainOrganization = new Organization(organization.Id, organization.Name);
        var serviceName = "Test Service";
        var averageTime = TimeSpan.FromMinutes(30);
        var windowNumbers = new List<int> { 1, 2, 3 };

        foreach (var windowNumber in windowNumbers)
        {
            var service = new Service(serviceName, averageTime);
            await organizationService.AddServiceAsync(domainOrganization, service, windowNumber);
        }

        var services = serviceRepository.GetAllByCondition(s => s.OrganizationId == organization.Id).ToList();
        Assert.NotEmpty(services);
        Assert.Contains(services, s => s.Name == serviceName);

        foreach (var windowNumber in windowNumbers)
        {
            var queueServiceLinks = queueServicesRepository
                .GetAllByCondition(qs => qs.ServiceId == services.First().Id && qs.QueueId == windowNumber)
                .ToList();

            Assert.NotEmpty(queueServiceLinks);
            Assert.Contains(queueServiceLinks, qs => qs.QueueId == windowNumber);
        }
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