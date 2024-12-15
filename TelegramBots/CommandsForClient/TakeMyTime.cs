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
        string messageText)
    {
        var result = await _queries.GetClientTimeQuery(chatId);
        if (result.IsFailure)
        {
            await botClient.SendTextMessageAsync(chatId, result.Error);
            return;
        }
            
        _myTime = result.Value;
        await botClient.SendTextMessageAsync(chatId, $"Ваше время ожидания составляет {_myTime}");
        userStates[chatId] = UserState.ClientStart;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForMyTime;
    }
}
