using Telegram.Bot;

namespace TelegramBots.Command;

public class RegisterClient : ICommand
{
    private readonly Dictionary<long, string> _organization;

    public RegisterClient(Dictionary<long, string> organization)
    {
        _organization = organization;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText)
    {
        _organization[chatId] = messageText;
        await botClient.SendTextMessageAsync(chatId, "Введите название услуги");
        userStates[chatId] = UserState.WaitingClientForNameService;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForNameOrganization;
    }
}
