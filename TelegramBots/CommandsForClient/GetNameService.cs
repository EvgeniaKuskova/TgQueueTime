using Domain;
using System.Threading;
using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class GetNameService : ICommand
{
    private readonly Dictionary<long, string> _organization;
    private readonly Commands _commands;
    private readonly Queries _queries;
    private TimeSpan _myTime;

    public GetNameService(Dictionary<long, string> organization, Commands commands, Queries queries)
    {
        _organization = organization;
        _commands = commands;
        _queries = queries;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText, CancellationToken cancellationToken)
    {
        var nameService = messageText;
        var services = await _queries.GetAllServices(_organization[chatId]);
        if (services.IsFailure)
        {
            await botClient.SendTextMessageAsync(chatId, services.Error);
            return;
        }
        
        var nameAllServices = services.Value.Select(x => x.Name).ToArray();
        if (!Array.Exists(nameAllServices, name => name == messageText))
        {
            await botClient.SendTextMessageAsync(chatId, "Такой услуги не существует, повторите ввод");
            userStates[chatId] = UserState.WaitingClientForNameService;
            return;
        }
        var result = await _commands.AddClientToQueueCommand(chatId, nameService, _organization[chatId]);

        if (result.IsFailure)
        {
            await botClient.SendTextMessageAsync(chatId, result.Error);
            return;
        }
        
        try
        {
            queueIsStarted = await _queries.IsQueueStarted(_organization[chatId], chatId);
            _myTime = await _queries.GetClientTimeQuery(chatId);
        }
        catch (Exception as e){
            
        }


        if (queueIsStarted)
        {
            var resultTime = _myTime.Hours == 0 ? $"{_myTime.Minutes} минут" : $"{_myTime.Hours} часов {_myTime.Minutes} минут";
            await botClient.SendTextMessageAsync(chatId,
                $"Вы зарегистрированы \nВаше время ожидания составляет {resultTime}. Отслеживайте его по команде /mytime.\n " +
                $"Количество клиентов до вас можно посмотреть по команде /clientsbeforeme");
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId,
                $"Вы зарегистрированы \nНо очередь еще не запущена. Ваше время /mytime пока отображается как 0 минут, после открытия оно изменится." +
                $" Отслеживайте его по команде /mytime.\n " +
                $"Количество клиентов до вас можно посмотреть по команде /clientsbeforeme");
        }
        

        _ = Task.Run(() => CheckClientTimeAsync(botClient, chatId, cancellationToken));
        userStates[chatId] = UserState.ClientStart;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForNameService;
    }

    private async Task CheckClientTimeAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await CheckTimeAndNotifyAsync(botClient, chatId, cancellationToken, async (myTime) =>
        {
            await botClient.SendTextMessageAsync(chatId, $"Уведомление! " +
                $"\nОчередь открыта. Ваше время ожидания составляет {myTime.Minutes} минут");
            return true;
        });

        await CheckTimeAndNotifyAsync(botClient, chatId, cancellationToken, async (myTime) =>
        {
            if (myTime.Minutes <= 4)
                return true;

            var fiveMinutesMore = new TimeSpan(0, 5, 10);
            var fiveMinutes = new TimeSpan(0, 5, 0);

            if (myTime <= fiveMinutesMore && myTime >= fiveMinutes)
            {
                await botClient.SendTextMessageAsync(chatId, $"Уведомление! " +
                    $"\nВаше время ожидания составляет {myTime.Minutes} минут");
                return true;
            }

            return false;
        });

        await CheckTimeAndNotifyAsync(botClient, chatId, cancellationToken, async (myTime) =>
        {
            var nullfiveMinutesMore = new TimeSpan(0, 0, 10);
            var nullMinutes = new TimeSpan(0, 0, 0);
            if (myTime <= nullfiveMinutesMore && myTime >= nullMinutes)
            {
                await botClient.SendTextMessageAsync(chatId, $"Уведомление! " +
                    $"\nВаша очередь подошла");
                return true;
            }

            return false;
        });
    }

    private async Task CheckTimeAndNotifyAsync(
        ITelegramBotClient botClient,
        long chatId,
        CancellationToken cancellationToken,
        Func<TimeSpan, Task<bool>> notificationAction)
    {
        while (true)
        {
            var queueIsStarted = await _queries.IsQueueStarted(_organization[chatId], chatId);
            if (queueIsStarted)
            {
                try
                {
                    var myTime = await _queries.GetClientTimeQuery(chatId);
                    if (await notificationAction(myTime))
                        break;
                }
                catch
                {
                    break;
                }
            }
            await Task.Delay(1000, cancellationToken);
        }
    }
}
