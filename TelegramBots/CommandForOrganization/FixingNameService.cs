using Telegram.Bot;

namespace TelegramBots.Command;

public class FixingNameService: ICommand
{
    private readonly Dictionary<long, Dictionary<string, TimeSpan>> _serviceAverageTime;
    private readonly string _goodResponse;

    public FixingNameService(Dictionary<long, Dictionary<string, TimeSpan>> serviceAverageTime)
    {
        _serviceAverageTime = serviceAverageTime;
        _goodResponse = "Продолжим, введите среднее время обслуживания одного клиента (в минутах). " +
                    "Например: 15 минут";
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, 
        string messageText)
    {
        _serviceAverageTime[chatId] = new Dictionary<string, TimeSpan>
        {
            [messageText] = new()
        };
        await botClient.SendTextMessageAsync(chatId, _goodResponse);
        userStates[chatId] = UserState.WaitingForAverageTime;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNameService;
    }
}