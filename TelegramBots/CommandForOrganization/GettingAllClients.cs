using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        string messageText)
    {
        if (!int.TryParse(messageText, out var windowNumber))
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите номера окон через запятую");
            return;
        }

        userStates[chatId] = UserState.Start;
        List<Client> clients;
        try
        {
            var task = _queries.GetAllClientsInQueueQuery(chatId, windowNumber);
            clients = task.Result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            if (e is InvalidOperationException)
                await botClient.SendTextMessageAsync(chatId, e.Message);
            throw;
        }
        
        await botClient.SendTextMessageAsync(chatId, $"Список клиентов: {string.Join('\n', clients)}");
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNumberWindowGet;
    }
}