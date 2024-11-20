using Domain.Entities;
using Domain.Services;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TgQueueTime.Application;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=Infrastructure/Database/Database.db"));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        services.AddScoped<OrganizationService>();
        services.AddScoped<QueueService>();
        
        services.AddScoped<Commands>();
        services.AddScoped<Queries>();
    }
}