using Infrastructure.Entities;
namespace Infrastructure;
using Domain;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext //  разделить работу с БД(не завязать на sqlite), настройку бд вынести в Application
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
            .ValueGeneratedOnAdd(); // Автоинкремент для Id (убрать)

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
            .ValueGeneratedOnAdd(); // Автоинкремент для Id (убрать)

        modelBuilder.Entity<OrganizationEntity>()
            .Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<OrganizationEntity>()
            .Property(s => s.WindowCount)
            .IsRequired(); 
        
        // Настройка сущности QueueEntity
        modelBuilder.Entity<QueueEntity>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<QueueEntity>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd(); // Автоинкремент для Id

        modelBuilder.Entity<QueueEntity>()
            .HasOne<OrganizationEntity>()
            .WithMany()
            .HasForeignKey(s => s.OrganizationId);

        modelBuilder.Entity<QueueEntity>()
            .Property(s => s.WindowNumber)
            .IsRequired(); 
        
        // Настройка сущности QueueClientsEntity
        modelBuilder.Entity<QueueClientsEntity>()
            .HasKey(s => s.Id);
        
        modelBuilder.Entity<QueueClientsEntity>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd(); // Автоинкремент для Id
        
        modelBuilder.Entity<QueueClientsEntity>()
            .HasOne<QueueEntity>()
            .WithMany()
            .HasForeignKey(s => s.QueueId);

        modelBuilder.Entity<QueueClientsEntity>()
            .Property(s => s.UserId)
            .IsRequired();

        modelBuilder.Entity<QueueClientsEntity>()
            .Property(s => s.Position)
            .IsRequired(); 
        
        modelBuilder.Entity<QueueClientsEntity>()
            .Property(s => s.StartTime)
            .IsRequired(); 

        
        // Настройка сущности QueueServicesEntity
        modelBuilder.Entity<QueueServicesEntity>()
            .HasKey(s => s.Id);
        
        modelBuilder.Entity<QueueServicesEntity>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd(); // Автоинкремент для Id
        
        modelBuilder.Entity<QueueServicesEntity>()
            .HasOne<QueueEntity>()
            .WithMany()
            .HasForeignKey(s => s.QueueId);
        
        modelBuilder.Entity<QueueServicesEntity>()
            .HasOne<ServiceEntity>()
            .WithMany()
            .HasForeignKey(s => s.ServiceId);
        
        
    }
}
