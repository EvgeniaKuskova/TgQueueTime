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

        var clientBot = new ClientBot("",
            commands, queries);
        await clientBot.Run();

        //var organizationBot = new OrganizationBot("",
            //commands, queries);
        //await organizationBot.Run();
    }
}