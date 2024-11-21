using Telegram.Bot;

namespace TelegramBots.Command;

public class ClientBefore : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText)
    {
        //var countClientsBefore = GetNumberClientsBeforeQuery(chatId);
        await botClient.SendTextMessageAsync(chatId, "Количество клиентов до вас");
        userStates[chatId] = UserState.ClientStart;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForCountClientsBefore;
    }
}
