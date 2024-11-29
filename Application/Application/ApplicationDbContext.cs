using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TgQueueTime.Application;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Добавляем DbSet для каждой сущности
    public DbSet<OrganizationEntity> Organizations { get; set; }
    public DbSet<ServiceEntity> Services { get; set; }
    public DbSet<QueueEntity> Queues { get; set; }
    public DbSet<ClientsEntity> Clients { get; set; }
    public DbSet<QueueServicesEntity> QueueServices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var dbPath = Path.Combine(basePath, "..", "..", "..", "Infrastructure", "Database", "Database.db");
            dbPath = Path.GetFullPath("C:\\Users\\kostr\\DynamicQueueProject\\TgQueueTime\\Infrastructure\\Infrastructure\\Database\\Database.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath};");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка сущности ServiceEntity
        modelBuilder.Entity<ServiceEntity>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<ServiceEntity>()
            .Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<ServiceEntity>()
            .Property(s => s.AverageTime)
            .IsRequired();

        modelBuilder.Entity<ServiceEntity>()
            .HasOne<OrganizationEntity>()
            .WithMany()
            .HasForeignKey(s => s.OrganizationId);

        // Настройка сущности OrganizationEntity
        modelBuilder.Entity<OrganizationEntity>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<OrganizationEntity>()
            .Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Настройка сущности QueueEntity
        modelBuilder.Entity<QueueEntity>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<QueueEntity>()
            .Property(s => s.WindowNumber)
            .IsRequired();

        modelBuilder.Entity<QueueEntity>()
            .HasOne<OrganizationEntity>()
            .WithMany()
            .HasForeignKey(s => s.OrganizationId);

        // Настройка сущности ClientsEntity
        modelBuilder.Entity<ClientsEntity>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<ClientsEntity>()
            .Property(s => s.UserId)
            .IsRequired();

        modelBuilder.Entity<ClientsEntity>()
            .Property(s => s.Position)
            .IsRequired();

        modelBuilder.Entity<ClientsEntity>()
            .Property(s => s.StartTime);

        modelBuilder.Entity<ClientsEntity>()
            .HasOne<QueueServicesEntity>()
            .WithMany()
            .HasForeignKey(s => s.QueueServiceId);

        // Настройка сущности QueueServicesEntity
        modelBuilder.Entity<QueueServicesEntity>()
            .HasKey(s => s.Id);

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