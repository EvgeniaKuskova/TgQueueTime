using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        string messageText)
    {
        try
        {
            var task = _queries.GetNumberClientsBeforeQuery(chatId);
            countClientsBefore = task.Result;
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
            if (e is InvalidOperationException)
                await botClient.SendTextMessageAsync(chatId, e.Message);
            throw;
        }

        await botClient.SendTextMessageAsync(chatId, $"Количество клиентов до вас {countClientsBefore}");
        userStates[chatId] = UserState.ClientStart;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForCountClientsBefore;
    }
}
