using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class UpdatingAverageTime: ICommand
{
    private readonly Dictionary<long, string> _serviceAverageTimeUpdate;
    private readonly string _goodResponse;
    private readonly Commands _commands;
    
    public UpdatingAverageTime(Dictionary<long, string> serviceAverageTimeUpdate, Commands commands)
    {
        _serviceAverageTimeUpdate = serviceAverageTimeUpdate;
        _commands = commands;
        _goodResponse = "Время успешно обновлено";
    }
    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, 
        string messageText, CancellationToken cancellationToken)
    {
        var parts = messageText.Split(' ');
        if (!int.TryParse(parts[0], out var minutes) || minutes < 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите время в минутах");
            return;
        }
        userStates[chatId] = UserState.Start;
        var result = await _commands.UpdateServiceAverageTimeCommand(chatId, _serviceAverageTimeUpdate[chatId],
            new TimeSpan(0, minutes, 0));
        if (result.IsFailure)
        {
            await botClient.SendTextMessageAsync(chatId, result.Error);
            return;
        }
        await botClient.SendTextMessageAsync(chatId, _goodResponse);
        _serviceAverageTimeUpdate.Remove(chatId);
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForAverageTimeUpdate;
    }
}