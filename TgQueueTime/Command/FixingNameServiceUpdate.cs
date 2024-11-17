using Telegram.Bot;

namespace TgQueueTime.Command;

public class FixingNameServiceUpdate: ICommand
{
    private readonly Dictionary<long, string> _serviceAverageTimeUpdate;
    private readonly string _response;
    private readonly string _userMessage;
    
    public FixingNameServiceUpdate(Dictionary<long, string> serviceAverageTimeUpdate, string userMessage)
    {
        _serviceAverageTimeUpdate = serviceAverageTimeUpdate;
        _userMessage = userMessage;
        _response = "Отлично! Теперь введите среднее время обслуживания клиента (в минутах). " +
                    "Например: 15 минут";
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates)
    {
        _serviceAverageTimeUpdate[chatId] = _userMessage;
        await botClient.SendTextMessageAsync(chatId, _response);
        userStates[chatId] = UserState.WaitingForAverageTimeUpdate;
    }
}