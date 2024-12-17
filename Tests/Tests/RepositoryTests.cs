using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Infrastructure;
using Infrastructure.Repositories;
using TgQueueTime.Application;
using Xunit;

public class RepositoryTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Should_Add_Entity_To_Database()
    {
        var context = GetInMemoryDbContext();
        var organizationRepository = new Repository<OrganizationEntity>(context);
        var newOrganization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(newOrganization);

        var serviceRepository = new Repository<ServiceEntity>(context);
        var newService = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:30:00",
            OrganizationId = 1L
        };
        await serviceRepository.AddAsync(newService);
        var entityInDb = await context.Set<ServiceEntity>().FindAsync(1L);
        Assert.NotNull(entityInDb);
        Assert.Equal("Test Service", entityInDb.Name);
    }


    [Fact]
    public async Task DeleteAsync_Should_Remove_Entity_From_Database()
    {
        var context = GetInMemoryDbContext();
        var organizationRepository = new Repository<OrganizationEntity>(context);
        var newOrganization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(newOrganization);

        var serviceRepository = new Repository<ServiceEntity>(context);
        var newService = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:30:00",
            OrganizationId = 1L
        };
        await serviceRepository.AddAsync(newService);
        await context.SaveChangesAsync();

        await serviceRepository.DeleteAsync(1L);

        var entityInDb = await context.Set<ServiceEntity>().FindAsync(newService.Id);
        Assert.Null(entityInDb);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Entity_In_Database()
    {
        var context = GetInMemoryDbContext();
        var organizationRepository = new Repository<OrganizationEntity>(context);
        var newOrganization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(newOrganization);

        var serviceRepository = new Repository<ServiceEntity>(context);
        var newService = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:30:00",
            OrganizationId = 1L
        };
        await serviceRepository.AddAsync(newService);
        await context.SaveChangesAsync();

        newService.Name = "Updated Name";
        await serviceRepository.UpdateAsync(newService);

        var entityInDb = await context.Set<ServiceEntity>().FindAsync(newService.Id);
        Assert.NotNull(entityInDb);
        Assert.Equal("Updated Name", entityInDb.Name);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Entity_If_It_Exists()
    {
        var context = GetInMemoryDbContext();
        var organizationRepository = new Repository<OrganizationEntity>(context);
        var newOrganization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await organizationRepository.AddAsync(newOrganization);

        var serviceRepository = new Repository<ServiceEntity>(context);
        var newService = new ServiceEntity
        {
            Name = "Existing Entity",
            AverageTime = "00:30:00",
            OrganizationId = 1L
        };
        await serviceRepository.AddAsync(newService);
        await context.SaveChangesAsync();

        var entityInDb = await serviceRepository.GetByKeyAsync(newService.Id);

        Assert.NotNull(entityInDb);
        Assert.Equal("Existing Entity", entityInDb.Name);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_If_Entity_Does_Not_Exist()
    {
        var context = GetInMemoryDbContext();
        var repository = new Repository<ServiceEntity>(context);

        var entityInDb = await repository.GetByKeyAsync(5);

        Assert.Null(entityInDb);
    }

    [Fact]
    public async Task GetAllByValueAsync_Should_Return_Entities_With_Matching_Value()
    {
        var context = GetInMemoryDbContext();
        var repository = new Repository<ServiceEntity>(context);
        var service1 = new ServiceEntity { Name = "Service1", AverageTime = "00:30:00", OrganizationId = 1 };
        var service2 = new ServiceEntity { Name = "Service1", AverageTime = "01:00:00", OrganizationId = 2 };
        var service3 = new ServiceEntity { Name = "Service2", AverageTime = "00:30:00", OrganizationId = 1 };

        await repository.AddAsync(service1);
        await repository.AddAsync(service2);
        await repository.AddAsync(service3);
        await context.SaveChangesAsync();

        var result = repository.GetAllByValueAsync(s => s.Name, "Service1").ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.OrganizationId == 1);
        Assert.Contains(result, s => s.OrganizationId == 2);
    }

    [Fact]
    public async Task GetByConditionsAsync_Should_Return_Entity_Matching_Condition()
    {
        var context = GetInMemoryDbContext();
        var repository = new Repository<ServiceEntity>(context);
        var service1 = new ServiceEntity { Name = "Service1", AverageTime = "00:30:00", OrganizationId = 1 };
        var service2 = new ServiceEntity { Name = "Service2", AverageTime = "01:00:00", OrganizationId = 2 };
        await repository.AddAsync(service1);
        await repository.AddAsync(service2);
        await context.SaveChangesAsync();

        var result = await repository.GetByConditionsAsync(s => s.OrganizationId == 2);

        Assert.NotNull(result);
        Assert.Equal("Service2", result.Name);
    }

    [Fact]
    public async Task GetAllByCondition_Should_Return_All_Entities_Matching_Condition()
    {
        var context = GetInMemoryDbContext();
        var repository = new Repository<ServiceEntity>(context);
        var service1 = new ServiceEntity { Name = "Service1", AverageTime = "00:30:00", OrganizationId = 1 };
        var service2 = new ServiceEntity { Name = "Service1", AverageTime = "01:00:00", OrganizationId = 2 };
        await repository.AddAsync(service1);
        await repository.AddAsync(service2);
        await context.SaveChangesAsync();

        var result = repository.GetAllByCondition(s => s.Name == "Service1").ToList();

        Assert.Equal(2, result.Count);
    }
}