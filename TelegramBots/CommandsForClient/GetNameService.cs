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

        await botClient.SendTextMessageAsync(chatId, "Вы зарегистрированы");
        userStates[chatId] = UserState.ClientStart;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForNameService;
    }
}
