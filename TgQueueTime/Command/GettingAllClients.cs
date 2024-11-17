using Telegram.Bot;

namespace TgQueueTime.Command;

public class GettingAllClients: ICommand
{
    private readonly string _response;
    private readonly string _userMessage;
    
    public GettingAllClients(string userMessage)
    {
        _userMessage = userMessage;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates)
    {
        if (!int.TryParse(_userMessage, out var number))
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите номер окна");
            return;
        }

        userStates[chatId] = UserState.Start;
        //var clients = GetAllClientsInQueueQuery(chat.Id, number);
        await botClient.SendTextMessageAsync(chatId, "Список клиентов");
    }
}