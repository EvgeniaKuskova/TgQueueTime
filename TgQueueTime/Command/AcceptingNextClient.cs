using Telegram.Bot;

namespace TgQueueTime.Command;

public class AcceptingNextClient: ICommand
{
    private readonly string _goodResponse = "Следующий клиент получил уведомление!";
    
    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, 
        string messageText)
    {
        if (!int.TryParse(messageText, out var number))
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите номер окна");
            return;
        }
        
        userStates[chatId] = UserState.Start;
        //var clients = GetAllClientsInQueueQuery(chat.Id, number);
        await botClient.SendTextMessageAsync(chatId, _goodResponse);
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNumberWindowToAccept;
    }
}