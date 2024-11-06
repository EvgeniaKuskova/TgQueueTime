namespace TgQueueTime;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class OrganizationBot
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;
    private static Dictionary<long, State> userStates = new();
    private static Dictionary<long, string> Names = new();
    private static Dictionary<long, string> ServiceAverageTimeUpdate = new();
    
    public static async Task Run()
    {
        _botClient = new TelegramBotClient(""); 
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery
            },
            ThrowPendingUpdates = true, 
        };
        
        using var cts = new CancellationTokenSource();
        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);
        var me = await _botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"{me.FirstName} запущен!");
        await Task.Delay(-1);
    }
    
    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    var message = update.Message;
                    var chat = message.Chat;
                    userStates.TryAdd(chat.Id, State.Start);
                    switch (message.Type)
                    {
                        case MessageType.Text:
                        {
                            await HandleTextMessage(botClient, chat, message.Text);
                            break;
                        }
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

    private static async Task HandleTextMessage(ITelegramBotClient botClient, Chat chat, string text)
    {
        var state = userStates[chat.Id];

        if (text == "/return")
        {
            userStates[chat.Id] = State.Start;
            await botClient.SendTextMessageAsync(
                chat.Id,
                $"Добро пожаловать в панель управления электронной очередью TgQueueTime!\n" +
                $"Я бот, который поможет вам эффективно управлять очередями и обслуживать клиентов. " +
                $"Для более подробной информации, что я умею, введи команду /help");
            return;
        }
        
        switch (state)
        {
            case State.Start:
                if (text == "/start")
                    await botClient.SendTextMessageAsync(
                        chat.Id,
                        $"Добро пожаловать в панель управления электронной очередью TgQueueTime!\n" +
                        $"Я бот, который поможет вам эффективно управлять очередями и обслуживать клиентов. " +
                        $"Для более подробной информации, что я умею, введи команду /help");
                            
                else if (text == "/help")
                {
                    await botClient.SendTextMessageAsync(
                        chat.Id,
                        $"Список доступных команд:\n" +
                        $"/registration - регистрация новой компании\n" +
                        $"/get_all_clients - получить список всех клиентов в очереди\n" +
                        $"/change_average_time - изменить среднее время для услуги\n" +
                        $"/return");
                }
                
                else if (text == "/registration")
                {
                    await botClient.SendTextMessageAsync(chat.Id,
                        "Введите название вашей организации, по которой ваши клиенты вас распознают");
                    userStates[chat.Id] = State.WaitingForNameOrganization;
                }

                else if (text == "/get_all_clients")
                {
                    await botClient.SendTextMessageAsync(chat.Id,
                        "Введите номер окна");
                    userStates[chat.Id] = State.WaitingForNumberWindowGet;
                }
                
                else if (text == "/change_average_time")
                {
                    await botClient.SendTextMessageAsync(chat.Id,
                        "Введите название услуги");
                    userStates[chat.Id] = State.WaitingForNameServiceUpdate;
                }
                
                break;
            
            case State.WaitingForNameOrganization:
                Names[chat.Id] = text;
                await botClient.SendTextMessageAsync(chat.Id, $"Введите название услуги");
                userStates[chat.Id] = State.WaitingForNameService;
                break;
            
            
            case State.WaitingForNameServiceUpdate:
                ServiceAverageTimeUpdate[chat.Id] = text;
                await botClient.SendTextMessageAsync(chat.Id, "Отлично! Теперь введите среднее время обслуживания клиента (в минутах). " +
                                                              "Например: 15 минут");
                userStates[chat.Id] = State.WaitingForAverageTimeUpdate;
                break;
            
            case State.WaitingForAverageTimeUpdate:
                var parts = text.Split(' ');
                if (!int.TryParse(parts[0], out var minutes))
                {
                    await botClient.SendTextMessageAsync(chat.Id, "Неккоректный ввод. Введите время в минутах");
                    break;
                }

                userStates[chat.Id] = State.Start;
                //var responce = UpdateServiceAverageTimeCommand(chat.Id, ServiceAverageTimeUpdate[chat.Id],
                //    new TimeSpan(0, minutes, 0));
                await botClient.SendTextMessageAsync(chat.Id, "Время успешно обновлено");
                break; 
            
            case State.WaitingForNumberWindowGet:
                if (!int.TryParse(text, out var number))
                {
                    await botClient.SendTextMessageAsync(chat.Id, "Неккоректный ввод. Введите номер окна");
                    break;
                }

                userStates[chat.Id] = State.Start;
                //var clients = GetAllClientsInQueueQuery(chat.Id, number);
                await botClient.SendTextMessageAsync(chat.Id, "Список клиентов");
                break;
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

    private enum State
    {
        Start,
        WaitingForNameOrganization,
        WaitingForNameService,
        WaitingForAverageTime,
        WaitingForNumberWindow,
        WaitingForAverageTimeUpdate,
        WaitingForNameServiceUpdate,
        WaitingForNumberWindowGet
    }
}