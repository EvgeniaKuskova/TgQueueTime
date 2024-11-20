using Telegram.Bot;

namespace TgQueueTime.Command;

public class RegisterOrganization: ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, 
        string messageText)
    {
        //TO DO
        await botClient.SendTextMessageAsync(chatId,
            "Поздравляю! Ваша организация успешно зарегистрирована.");
        //Console.WriteLine(messageText);
        userStates[chatId] = UserState.Start;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNameOrganization;
    }
}