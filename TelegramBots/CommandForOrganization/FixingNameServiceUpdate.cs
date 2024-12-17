using Telegram.Bot;

namespace TelegramBots.Command;

public class FixingNameServiceUpdate: ICommand
{
    private readonly Dictionary<long, string> _serviceAverageTimeUpdate;
    private readonly string _goodResponse;
    
    public FixingNameServiceUpdate(Dictionary<long, string> serviceAverageTimeUpdate)
    {
        _serviceAverageTimeUpdate = serviceAverageTimeUpdate;
        _goodResponse = "Отлично! Теперь введите среднее время обслуживания клиента (в минутах). " +
                    "Например: 15 минут";
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText, CancellationToken cancellationToken)
    {
        _serviceAverageTimeUpdate[chatId] = messageText;
        await botClient.SendTextMessageAsync(chatId, _goodResponse);
        userStates[chatId] = UserState.WaitingForAverageTimeUpdate;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNameServiceUpdate;
    }
}