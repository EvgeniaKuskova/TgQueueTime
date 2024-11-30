using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class AcceptingNextClient: ICommand
{
    private readonly string _goodResponse = "Следующий клиент получил уведомление!";
    private readonly Commands _commands;

    public AcceptingNextClient(Commands commands)
    {
        _commands = commands;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, 
        string messageText)
    {
        if (!int.TryParse(messageText, out var windowNumber))
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите номер окна");
            return;
        }
        
        userStates[chatId] = UserState.Start;
        try
        {
            await _commands.MoveQueue(chatId, windowNumber);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            if (e is InvalidOperationException)
                await botClient.SendTextMessageAsync(chatId, e.Message);
            throw;
        }
        await botClient.SendTextMessageAsync(chatId, _goodResponse);
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNumberWindowToAccept;
    }
}