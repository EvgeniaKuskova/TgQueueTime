namespace TgQueueTime;

    class Program
    {
        static async Task Main()
        {
           var organizationBot = new OrganizationBot("7547068208:AAGTkGqApjrfi0J6P9JXR8ZE1QdE08lRV6E");
           await organizationBot.Run();
        }
    }

