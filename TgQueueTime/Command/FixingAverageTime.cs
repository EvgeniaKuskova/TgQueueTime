using Telegram.Bot;

namespace TgQueueTime.Command;

public class FixingAverageTime: ICommand
{
    private readonly Dictionary<long, Dictionary<string, TimeSpan>> _serviceAverageTime;
    private readonly string _response;
    private readonly string _userMessage;

    public FixingAverageTime(Dictionary<long, Dictionary<string, TimeSpan>> serviceAverageTime, string userMessage)
    {
        _serviceAverageTime = serviceAverageTime;
        _userMessage = userMessage;
        _response = "Теперь введите список окон, где будет предоставляться эта услуга (через запятую). " +
                    "Например: 1, 2, 3";
    }
    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates)
    {
        var parts = _userMessage.Split(' ');
        if (!int.TryParse(parts[0], out var minute) || minute < 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите время в минутах");
            return;
        }
        var serviceName = _serviceAverageTime[chatId].Keys.First();
        _serviceAverageTime[chatId][serviceName] = new TimeSpan(0, minute, 0);
        await botClient.SendTextMessageAsync(chatId, _response);
        userStates[chatId] = UserState.WaitingForNumbersWindow;
    }
}