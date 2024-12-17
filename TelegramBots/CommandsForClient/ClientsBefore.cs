using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class ClientBefore : ICommand
{
    private readonly Queries _queries;
    private int countClientsBefore = -1;

    public ClientBefore(Queries queries)
    {
        _queries = queries;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText, CancellationToken cancellationToken)
    {
        var result = await _queries.GetNumberClientsBeforeQuery(chatId);
        if (result.IsFailure)
        {
            await botClient.SendTextMessageAsync(chatId, result.Error);
            return;
        }
        countClientsBefore = result.Value;
        await botClient.SendTextMessageAsync(chatId, $"Количество клиентов до вас {countClientsBefore}");
        userStates[chatId] = UserState.ClientStart;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForCountClientsBefore;
    }
}
