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
        var clientBot = new ClientBot("7649309220:AAEYnahNNtjr4KwGMk0AICh_TUSYVrnH_4k",
            commands, queries);
        var organizationBot = new OrganizationBot("7547068208:AAFlmL6SSlr9YTMk1K_02fhdnXJtvXpsANk",
            commands, queries);
        
        await Task.WhenAll(clientBot.Run(), organizationBot.Run());
    }
}