using Telegram.Bot;

namespace TelegramBots.Command;

public class GetNameService : ICommand
{
    private readonly Dictionary<long, string> _organization;

    public GetNameService(Dictionary<long, string> organization)
    {
        _organization = organization;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText)
    {
        var nameService = messageText;
        //AddClientToQueueCommand(chatId, nameService, _organization[chatId]);
        await botClient.SendTextMessageAsync(chatId, "Вы зарегистрированы");
        userStates[chatId] = UserState.ClientStart;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForNameService;
    }
}
