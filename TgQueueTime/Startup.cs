﻿using Domain.Services;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TgQueueTime.Application;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var dbPath =
            "C:\\Users\\КусЯ\\Desktop\\TgQueueTime\\TgQueueTimeMeow\\Infrastructure\\Infrastructure\\Database\\Database.db";
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath};Cache=Shared"));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        services.AddScoped<OrganizationService>();
        services.AddScoped<QueueService>();
        
        services.AddScoped<Commands>();
        services.AddScoped<Queries>();
        
        services.AddScoped<DbContext, ApplicationDbContext>();
    }
}