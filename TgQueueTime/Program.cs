namespace TgQueueTime;

    class Program
    {
        static async Task Main()
        {
           var organizationBot = new OrganizationBot("7547068208:AAEf6qV7g56j4NJ6SKK-yKtRIunvrZJsxqo");
           await organizationBot.Run();
        }
    }

