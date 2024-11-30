using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class RegisterOrganization: ICommand
{
    private readonly Commands _commands;
    private readonly string _goodResponse;
    private readonly string _badResponse;

    public RegisterOrganization(Commands commands)
    {
        _commands = commands;
        _goodResponse = "Поздравляю! Ваша организация успешно зарегистрирована.";
        _badResponse = "Извините, но что-то пошло не так. Попробуйте еще раз";
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, 
        string messageText)
    {

        try
        {
            await _commands.RegisterOrganizationCommand(chatId, messageText);
        }
        
        catch (Exception e)
        {
            Console.WriteLine(e);
            await botClient.SendTextMessageAsync(chatId, _badResponse);
            throw;
        }
        
        await botClient.SendTextMessageAsync(chatId, _goodResponse);
        userStates[chatId] = UserState.Start;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNameOrganization;
    }
}