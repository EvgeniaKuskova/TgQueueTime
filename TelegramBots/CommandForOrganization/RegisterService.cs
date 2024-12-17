using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class RegisterService: ICommand
{
    private readonly Dictionary<long, Dictionary<string, TimeSpan>> _serviceAverageTime;
    private readonly string _goodResponse;
    private readonly Commands _commands;

    public RegisterService(Dictionary<long, Dictionary<string, TimeSpan>> serviceAverageTime, Commands commands)
    {
        _serviceAverageTime = serviceAverageTime;
        _commands = commands;
        _goodResponse = "Услуга успешно добавлена!";
    }
    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText, CancellationToken cancellationToken)
    {
        var windows = messageText.Split(',').Select(x => x.Trim()).ToList();
        if (windows.Any(x => !int.TryParse(x, out _)))
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите номера окон через запятую");
            return;
        }
        var name = _serviceAverageTime[chatId].Keys.First();
        
        try
        {
            await _commands.AddService(chatId, name, _serviceAverageTime[chatId][name],
                windows.Select(int.Parse).ToList());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            if (e is InvalidOperationException)
                await botClient.SendTextMessageAsync(chatId, e.Message);
            throw;
        }
        await botClient.SendTextMessageAsync(chatId, _goodResponse);
        _serviceAverageTime.Remove(chatId);
        userStates[chatId] = UserState.Start;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNumbersWindow;
    }
}