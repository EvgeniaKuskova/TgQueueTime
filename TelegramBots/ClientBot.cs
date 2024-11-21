using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;


namespace TelegramBots;
public class ClientBot
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUpdateHandler _updateHandler;

    public ClientBot(string token)
    {
        _botClient = new TelegramBotClient(token);
        _updateHandler = new ClientUpdateHandler(_botClient);
    }

    public async Task Run()
    {
        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        _botClient.StartReceiving(_updateHandler.HandleUpdateAsync, ErrorHandler, receiverOptions, cts.Token);
        var me = await _botClient.GetMeAsync(cts.Token);
        Console.WriteLine($"{me.FirstName} запущен!");
        await Task.Delay(-1);
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var errorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
