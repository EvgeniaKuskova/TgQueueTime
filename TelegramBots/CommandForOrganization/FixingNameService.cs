using Telegram.Bot;

namespace TelegramBots.Command;

public class FixingNameService: ICommand
{
    private readonly Dictionary<long, Dictionary<string, TimeSpan>> _serviceAverageTime;
    private readonly string _response;

    public FixingNameService(Dictionary<long, Dictionary<string, TimeSpan>> serviceAverageTime)
    {
        _serviceAverageTime = serviceAverageTime;
        _response = "Продолжим, введите среднее время обслуживания одного клиента (в минутах). " +
                    "Например: 15 минут";
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, 
        string messageText, CancellationToken cancellationToken)
    {
        _serviceAverageTime[chatId] = new Dictionary<string, TimeSpan>
        {
            [messageText] = new()
        };
        await botClient.SendTextMessageAsync(chatId, _response);
        userStates[chatId] = UserState.WaitingForAverageTime;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNameService;
    }
}