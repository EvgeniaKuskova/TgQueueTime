using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class GetNameService : ICommand
{
    private readonly Dictionary<long, string> _organization;
    private readonly Commands _commands;

    public GetNameService(Dictionary<long, string> organization, Commands commands)
    {
        _organization = organization;
        _commands = commands;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText)
    {
        var nameService = messageText;
        try
        {
            await _commands.AddClientToQueueCommand(chatId, nameService, _organization[chatId]);
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
            await botClient.SendTextMessageAsync(chatId, "Извните, сохранить услугу не получилось.");
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
