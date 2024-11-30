using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TgQueueTime.Application;


namespace TelegramBots;
public class OrganizationBot
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUpdateHandler _updateHandler;
    private readonly Commands _commands;
    private readonly Queries _queries;
    
    public OrganizationBot(string token, Commands commands, Queries queries)
    {
        _commands = commands;
        _queries = queries;
        _botClient = new TelegramBotClient(token);
        _updateHandler = new UpdateHandler(_botClient, _commands, _queries);
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