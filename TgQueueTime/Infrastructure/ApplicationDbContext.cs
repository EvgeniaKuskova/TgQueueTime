using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Добавляем DbSet для каждой сущности
        public DbSet<OrganizationEntity> Organizations { get; set; }
        public DbSet<ServiceEntity> Services { get; set; }
        public DbSet<QueueEntity> Queues { get; set; }
        public DbSet<QueueClientsEntity> QueueClients { get; set; }
        public DbSet<QueueServicesEntity> QueueServices { get; set; }

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

            // Настройка других сущностей аналогичным образом
            // ...
        }
    }
}
