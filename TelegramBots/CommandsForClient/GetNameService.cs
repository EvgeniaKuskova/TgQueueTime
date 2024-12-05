using Domain;
using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class GetNameService : ICommand
{
    private readonly Dictionary<long, string> _organization;
    private readonly Commands _commands;
    private readonly Queries _queries;

    public GetNameService(Dictionary<long, string> organization, Commands commands, Queries queries)
    {
        _organization = organization;
        _commands = commands;
        _queries = queries;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText)
    {
        var nameService = messageText;
        try
        {
            var task = _queries.GetAllServices(_organization[chatId]);
            var allServices = task.Result;
            var nameServices = allServices.Select(x => x.Name).ToArray();
            if (!(Array.Exists(nameServices, name => name == messageText)))
            {
                await botClient.SendTextMessageAsync(chatId, "Такой услуги не существует, повторите ввод");
                userStates[chatId] = UserState.WaitingClientForNameService;
                return;
            }
            await _commands.AddClientToQueueCommand(chatId, nameService, _organization[chatId]);
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
            if (e is InvalidOperationException)
                await botClient.SendTextMessageAsync(chatId, e.Message);
            throw;
        }

        await botClient.SendTextMessageAsync(chatId, "Вы зарегистрированы");
        userStates[chatId] = UserState.ClientStart;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForNameService;
    }
}
