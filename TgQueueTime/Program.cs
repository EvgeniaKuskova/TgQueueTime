namespace TgQueueTime;

    class Program
    {
        static async Task Main()
        {
           var organizationBot = new OrganizationBot("");
           await organizationBot.Run();
        }
    }

