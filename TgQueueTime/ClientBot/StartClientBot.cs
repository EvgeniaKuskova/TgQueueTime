using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
namespace TgQueueTime.ClientBot;

using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class StartClientBot
{
    private static ITelegramBotClient botClient;
    private static ReceiverOptions receiverOptions;

    public static async Task Run()
    {
        var token = TakeToken();
        botClient = new TelegramBotClient(token);
        receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, },
            DropPendingUpdates = true,
        };

        using var cts = new CancellationTokenSource();
        botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);

        var bot = await botClient.GetMeAsync();
        Console.WriteLine($"{bot.FirstName} запущен!");

        await Task.Delay(-1);
    }

    public static string TakeToken()
    {
        string clientBotToken = Environment.GetEnvironmentVariable("tokenClientBotC#");
        if (string.IsNullOrEmpty(clientBotToken))
            throw new Exception("API token is not set in environment variables.");
        return clientBotToken;
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    var message = update.Message;
                    var chat = message.Chat;

                    switch (message.Type)
                    {
                        case MessageType.Text:
                            HandlerTextMessageText(botClient, message, chat);
                            return;
                    }

                    Console.WriteLine("Пришло сообщение!");
                    return;
            }
        }
        catch (Exception exeption)
        {
            Console.WriteLine(exeption.ToString());
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    private static async Task HandlerTextMessageText(ITelegramBotClient botClient, Message message, Chat chat)
    {
        var startMessage = "зарегистрируйтесь в очереди: /register" + "\n"
                                + "узнать время ожидания: /mytime" + "\n"
                                + "количество клиентов до меня: /clientsbeforeme" + "\n"
                                + "выйти из очереди: /quitqueue";
        var helpMessage = "Вы зашли в бота, где можно отслеживать очередь. Для того, чтобы начать введите /start";
        switch (message.Text)
        {
            case "/start":
                await botClient.SendTextMessageAsync(chat.Id, startMessage);
                return;
            case "/help":
                await botClient.SendTextMessageAsync(chat.Id, helpMessage);
                return;
            case "/register":
                await botClient.SendTextMessageAsync(chat.Id, "регистрация");
                return;
            case "/mytime":
                await botClient.SendTextMessageAsync(chat.Id, "оставшееся время");
                return;
            case "/clientsbeforeme":
                await botClient.SendTextMessageAsync(chat.Id, "клиенты до меня");
                return;
            case "/quitqueue":
                await botClient.SendTextMessageAsync(chat.Id, "выйти из очереди");
                return;
        }
        await botClient.SendTextMessageAsync(chat.Id, "команда не найдена");
        return;
    }
}

