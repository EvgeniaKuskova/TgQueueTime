using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class RegisterOrganization: ICommand
{
    private readonly Commands _commands;
    private readonly string _goodResponse;

    public RegisterOrganization(Commands commands)
    {
        _commands = commands;
        _goodResponse = "Поздравляю! Ваша организация успешно зарегистрирована.";
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, 
        string messageText)
    {
        var result = await _commands.RegisterOrganizationCommand(chatId, messageText);
        if (result.IsFailure)
        {
            await botClient.SendTextMessageAsync(chatId, result.Error);
            return;
        }
        
        await botClient.SendTextMessageAsync(chatId, _goodResponse);
        userStates[chatId] = UserState.Start;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNameOrganization;
    }
}