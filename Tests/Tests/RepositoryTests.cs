using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Infrastructure;
using Infrastructure.Repositories;
using Xunit;

public class RepositoryTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        /*var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var dbPath = Path.Combine(basePath, "..", "..", "..", "Infrastructure", "Database", "Database.db");
        dbPath = Path.GetFullPath(dbPath); // Получает абсолютный путь
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            //.UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseSqlite($"Data Source={dbPath};")
            .Options;

        return new ApplicationDbContext(options);*/
        
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
        
        var entityInDb = await serviceRepository.GetByIdAsync(newService.Id);
        
        Assert.NotNull(entityInDb);
        Assert.Equal("Existing Entity", entityInDb.Name);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_If_Entity_Does_Not_Exist()
    {
        var context = GetInMemoryDbContext();
        var repository = new Repository<ServiceEntity>(context);

        var entityInDb = await repository.GetByIdAsync(5);

        Assert.Null(entityInDb);
    }
}
