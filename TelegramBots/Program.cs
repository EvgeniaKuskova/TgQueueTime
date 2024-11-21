namespace TelegramBots;

internal static class Program
{
    static async Task Main()
    {
        var organizationBot = new OrganizationBot("");
        var clientBot = new ClientBot("");
        await organizationBot.Run();
        await clientBot.Run();
    }
}