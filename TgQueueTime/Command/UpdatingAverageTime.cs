using Telegram.Bot;

namespace TgQueueTime.Command;

public class UpdatingAverageTime: ICommand
{
    private readonly Dictionary<long, string> _serviceAverageTimeUpdate;
    private readonly string _goodResponse;
    private readonly string _userMessage;
    
    public UpdatingAverageTime(Dictionary<long, string> serviceAverageTimeUpdate, string userMessage)
    {
        _serviceAverageTimeUpdate = serviceAverageTimeUpdate;
        _userMessage = userMessage;
        _goodResponse = "Время успешно обновлено";
    }
    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates)
    {
        var parts = _userMessage.Split(' ');
        if (!int.TryParse(parts[0], out var minutes) || minutes < 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите время в минутах");
            return;
        }
        userStates[chatId] = UserState.Start;
        //var responce = UpdateServiceAverageTimeCommand(chat.Id, _serviceAverageTimeUpdate[chatId],
        //    new TimeSpan(0, minutes, 0));
        await botClient.SendTextMessageAsync(chatId, _goodResponse);
    }
}