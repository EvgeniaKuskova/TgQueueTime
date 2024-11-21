using Telegram.Bot;

namespace TelegramBots.Command;

public class TakeMyTime : ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText)
    {
        //var myTime = GetClientTimeQuery(chatId);
        await botClient.SendTextMessageAsync(chatId, "Ваше время ожидания составляет");
        userStates[chatId] = UserState.ClientStart;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForMyTime;
    }
}
