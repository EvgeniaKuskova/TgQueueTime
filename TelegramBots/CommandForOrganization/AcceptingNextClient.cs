using CSharpFunctionalExtensions;
using Domain;
using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class AcceptingNextClient: ICommand
{
    private readonly string _goodResponse;
    private readonly Commands _commands;

    public AcceptingNextClient(Commands commands)
    {
        _commands = commands;
        _goodResponse = "Следующий клиент получил уведомление!";
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
        var result = await _commands.MoveQueue(chatId, windowNumber);
        if (result.IsFailure)
        {
            await botClient.SendTextMessageAsync(chatId, result.Error);
            return;
        }
        
        await botClient.SendTextMessageAsync(chatId, _goodResponse);
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForNumberWindowToAccept;
    }
}