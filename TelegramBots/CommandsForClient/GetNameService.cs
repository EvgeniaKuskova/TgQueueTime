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
        bool queueIsStarted;
        try
        {
            var task = _queries.GetAllServices(_organization[chatId]);
            var allServices = task.Result;
            var nameServices = allServices.Select(x => x.Name).ToArray();
            if (!Array.Exists(nameServices, name => name == messageText))
            {
                await botClient.SendTextMessageAsync(chatId, "Такой услуги не существует, повторите ввод");
                userStates[chatId] = UserState.WaitingClientForNameService;
                return;
            }
            await _commands.AddClientToQueueCommand(chatId, nameService, _organization[chatId]);
            queueIsStarted = await _queries.IsQueueStarted(_organization[chatId], );
            _myTime = await _queries.GetClientTimeQuery(chatId);
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
            if (e is InvalidOperationException)
                await botClient.SendTextMessageAsync(chatId, e.Message);
            throw;
        }

        var resultTime = _myTime.Hours == 0 ? $"{_myTime.Minutes} минут" : $"{_myTime.Hours} часов {_myTime.Minutes} минут";
        await botClient.SendTextMessageAsync(chatId, 
            $"Вы зарегистрированы \nВаше время ожидания составляет {resultTime}. Отслеживайте его по команде /mytime.\n " +
            $"Количество клиентов до вас можно посмотреть по команде /clientsbeforeme");

        _ = Task.Run(() => CheckClientTimeAsync(botClient, chatId, cancellationToken, queueIsStarted));
        userStates[chatId] = UserState.ClientStart;

        
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForNameService;
    }

    private async Task CheckClientTimeAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, bool queueStarted)
    {
        while (true)
        {
            if (queueStarted)
            {
                try
                {
                    var _myTime = await _queries.GetClientTimeQuery(chatId);
                    if (_myTime.Minutes <= 4)
                        break;

                    var fiveMinutesMore = new TimeSpan(0, 5, 10);
                    var fiveMinutes = new TimeSpan(0, 5, 0);

                    if (_myTime <= fiveMinutesMore && _myTime > fiveMinutes)
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Уведомление! " +
                            $"\nВаше время ожидания составляет {_myTime.Minutes} минут");
                        break;
                    }
                    await Task.Delay(1000, cancellationToken);
                }
                catch
                {
                    break;
                }
            }
        }
        while (true)
        {
            if (queueStarted)
            {
                try
                {
                    var _myTime = await _queries.GetClientTimeQuery(chatId);

                    var nullfiveMinutesMore = new TimeSpan(0, 0, 10);
                    var nullMinutes = new TimeSpan(0, 0, 0);
                    if (_myTime <= nullfiveMinutesMore && _myTime >= nullMinutes)
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Уведомление! " +
                            $"\nВаше время ожидания составляет {_myTime.Minutes} минут");
                        break;
                    }
                    await Task.Delay(1000, cancellationToken);
                }
                catch
                {
                    break;
                }
            }
        }
    }
}
