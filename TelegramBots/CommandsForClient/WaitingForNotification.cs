using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class WaitingForNotification : ICommand
{
    private readonly Queries _queries;
    private TimeSpan _myTime;

    public WaitingForNotification(Queries queries)
    {
        _queries = queries;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText)
    {
        while (true)
        {
            try
            {
                var task = _queries.GetClientTimeQuery(chatId);
                _myTime = task.Result;
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                if (e is InvalidOperationException)
                    await botClient.SendTextMessageAsync(chatId, e.Message);
                throw;
            }
            var fiveMinutesMore = new TimeSpan(0, 5, 10);
            var fiveMinutes = new TimeSpan(0, 5, 0);
            if (_myTime < fiveMinutesMore && _myTime > fiveMinutes)
            {
                await botClient.SendTextMessageAsync(chatId, $"Ваше время ожидания составляет {_myTime.Minutes} минут");
                break;
            }
        }
        userStates[chatId] = UserState.ClientStart;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingNotification;
    }
}