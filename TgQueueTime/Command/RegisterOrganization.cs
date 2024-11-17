using Telegram.Bot;

namespace TgQueueTime.Command;

public class RegisterOrganization: ICommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates)
    {
        //TO DO
        await botClient.SendTextMessageAsync(chatId,
            "Поздравляю! Ваша организация успешно зарегистрирована.");
        userStates[chatId] = UserState.Start;
    }
}