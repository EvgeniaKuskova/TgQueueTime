using Infrastructure.Entities;
namespace Infrastructure;
using Domain;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{

    public ApplicationDbContext() { }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var dbPath = Path.Combine(basePath, "..", "..", "..", "Infrastructure", "Database", "Database.db");
            dbPath = Path.GetFullPath(dbPath);
            optionsBuilder.UseSqlite($"Data Source={dbPath};");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Настройка сущности ServiceEntity
        modelBuilder.Entity<ServiceEntity>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<ServiceEntity>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd(); // Автоинкремент для Id

        modelBuilder.Entity<ServiceEntity>()
            .Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<ServiceEntity>()
            .Property(s => s.AverageTime)
            .IsRequired(); // Хранится как строка

        modelBuilder.Entity<ServiceEntity>()
            .HasOne<OrganizationEntity>()
            .WithMany()
            .HasForeignKey(s => s.OrganizationId);
        
        // Настройка сущности OrganizationEntity
        modelBuilder.Entity<OrganizationEntity>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<OrganizationEntity>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd(); // Автоинкремент для Id

        modelBuilder.Entity<OrganizationEntity>()
            .Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<OrganizationEntity>()
            .Property(s => s.WindowCount)
            .IsRequired(); 
    }
}
