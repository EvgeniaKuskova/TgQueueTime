using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class TakeMyTime : ICommand
{
    private readonly Queries _queries;
    private TimeSpan _myTime;

    public TakeMyTime(Queries queries)
    {
        _queries = queries;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText, CancellationToken cancellationToken)
    {
        var result = await _queries.GetClientTimeQuery(chatId);
        if (result.IsFailure)
        {
            await botClient.SendTextMessageAsync(chatId, result.Error);
            return;
        }
            
        _myTime = result.Value;
        var hours = _myTime.Hours;
        var minutes = _myTime.Minutes;
        var resultTime = hours == 0 ? $"{_myTime.Minutes} минут" : $"{hours} часов {minutes} минут";
        await botClient.SendTextMessageAsync(chatId, $"Ваше время ожидания составляет {resultTime}");
        userStates[chatId] = UserState.ClientStart;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForMyTime;
    }
}
