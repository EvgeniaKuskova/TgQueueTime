using Microsoft.Extensions.DependencyInjection;
using TgQueueTime.Application;

namespace TelegramBots;

internal static class Program
{
    static async Task Main()
    {
        var services = new ServiceCollection(); 
        var startup = new Startup(); 
        startup.ConfigureServices(services); 
        var serviceProvider = services.BuildServiceProvider();
        var commands = serviceProvider.GetRequiredService<Commands>();
        var queries = serviceProvider.GetRequiredService<Queries>();
        
        var organizationBot = new OrganizationBot("7547068208:AAGon0KmcOaFpD2VJO_UmGiqzuSs86J6TBU",
            commands, queries);
        await organizationBot.Run();
    }
}