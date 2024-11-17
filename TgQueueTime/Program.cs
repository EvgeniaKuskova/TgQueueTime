namespace TgQueueTime;

    class Program
    {
        static async Task Main()
        {
           var organizationBot = new OrganizationBot("7547068208:AAHLfLIFPWMkZwgMmHscbL53f_Ul-Z6sCJI");
           await organizationBot.Run();
        }
    }

