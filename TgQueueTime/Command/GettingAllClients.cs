using Telegram.Bot;

namespace TgQueueTime.Command;

public class GettingAllClients: ICommand
{

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText)
    {
        if (!int.TryParse(messageText, out var number))
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите номера окон через запятую");
            return;
        }

        userStates[chatId] = UserState.Start;
        //var clients = GetAllClientsInQueueQuery(chat.Id, number);
        //Console.WriteLine(number);
        await botClient.SendTextMessageAsync(chatId, "Список клиентов");
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNumberWindowGet;
    }
}