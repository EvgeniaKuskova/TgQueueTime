using Domain;
using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class GettingAllClients: ICommand
{
    private readonly Queries _queries;

    public GettingAllClients(Queries queries)
    {
        _queries = queries;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText, CancellationToken cancellationToken)
    {
        if (!int.TryParse(messageText, out var windowNumber))
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите номера окон через запятую");
            return;
        }

        userStates[chatId] = UserState.Start;
        var result = await _queries.GetAllClientsInQueueQuery(chatId, windowNumber);
        if (result.IsFailure)
        {
            await botClient.SendTextMessageAsync(chatId, result.Error);
            return;
        }

        var clients = result.Value;
        await botClient.SendTextMessageAsync(chatId, $"Список клиентов: {string.Join('\n', clients)}");
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNumberWindowGet;
    }
}