namespace TelegramBots;

internal static class Program
{
    static async Task Main()
    {
        var organizationBot = new OrganizationBot("");
        await organizationBot.Run();
    }
}