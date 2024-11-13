using Domain.Services;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=Database.db"));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Регистрация доменных сервисов
        services.AddScoped<IQueueService, QueueService>();
        services.AddScoped<IOrganizationService, OrganizationService>();

        return services.BuildServiceProvider();
    }
}