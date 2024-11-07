using TgQueueTime.ClientBot;

namespace TgQueueTime;

class Program
{
    static async Task Main()
    {
        await StartClientBot.Run();
    }
}