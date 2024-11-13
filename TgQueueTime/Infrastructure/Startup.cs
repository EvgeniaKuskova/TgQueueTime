using Domain;
using Infrastructure.Entities;

namespace Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Repositories;

public class Startup
{
    public static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Настройка DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=Database.db"));

        // Регистрация репозитория и маппера
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IEntityMapper<,>), typeof(EntityMapperBase<,>));

        // Регистрация DomainService
        services.AddScoped(typeof(IDomain<,>), typeof(DomainService<,>));

        return services.BuildServiceProvider();
    }
}
