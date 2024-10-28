using Telegram.Bot.Types.ReplyMarkups;

namespace TgQueueTime;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public static class CompanyBot
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;
    
    public static async Task Run()
    {
        _botClient = new TelegramBotClient("7547068208:AAHwMG6hJOWWjyRrL2CWtebiUmj6SiM13QA"); 
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
            {
                UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
                UpdateType.CallbackQuery
            },
            // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
            // True - не обрабатывать, False (стоит по умолчанию) - обрабаывать
            ThrowPendingUpdates = true, 
        };
        
        using var cts = new CancellationTokenSource();
        
        // UpdateHander - обработчик приходящих Update`ов
        // ErrorHandler - обработчик ошибок, связанных с Bot API
        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота
        
        var me = await _botClient.GetMeAsync(cancellationToken: cts.Token); // Создаем переменную, в которую помещаем информацию о нашем боте.
        Console.WriteLine($"{me.FirstName} запущен!");
        
        await Task.Delay(-1); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно
    }
    
    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Обязательно ставим блок try-catch, чтобы наш бот не "падал" в случае каких-либо ошибок
        try
        {
            // Сразу же ставим конструкцию switch, чтобы обрабатывать приходящие Update
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    var message = update.Message;
                    var chat = message.Chat;
                    switch (message.Type)
                    {
                        case MessageType.Text:
                        {
                            if (message.Text == "/start")
                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    $"Добро пожаловать в панель управления электронной очередью TgQueueTime!\n" +
                                    $"Я бот, который поможет вам эффективно управлять очередями и обслуживать клиентов. " +
                                    $"Для более подробной информации, что я умею, введи команду /help");
                            
                            else if (message.Text == "/help")
                            {
                                var inlineKeyboard = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Регистрация")
                                        }
                                    });
                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    "Можно зарегистрироваться!",
                                    replyMarkup: inlineKeyboard);
                            }
                            return;
                        }
                    }
                    
                    return;
                }
                
                case UpdateType.CallbackQuery:
                {
                    var callbackQuery = update.CallbackQuery;
                    var chat = callbackQuery.Message.Chat;
                    switch (callbackQuery.Data)
                    {
                        case "Регистрация":
                            await botClient.SendTextMessageAsync(
                                chat.Id,
                                "/registration");
                            return;
                    }
                    
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
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