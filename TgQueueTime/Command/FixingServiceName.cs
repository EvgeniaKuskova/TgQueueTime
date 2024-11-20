using Telegram.Bot;

namespace TgQueueTime.Command;

public class FixingServiceName: ICommand
{
    private readonly Dictionary<long, Dictionary<string, TimeSpan>> _serviceAverageTime;
    private readonly string _response;

    public FixingServiceName(Dictionary<long, Dictionary<string, TimeSpan>> serviceAverageTime)
    {
        _serviceAverageTime = serviceAverageTime;
        _response = "Продолжим, введите среднее время обслуживания одного клиента (в минутах). " +
                    "Например: 15 минут";
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, 
        string messageText)
    {
        _serviceAverageTime[chatId] = new Dictionary<string, TimeSpan>
        {
            [messageText] = new()
        };
        await botClient.SendTextMessageAsync(chatId, _response);
        userStates[chatId] = UserState.WaitingForAverageTime;
    }
}