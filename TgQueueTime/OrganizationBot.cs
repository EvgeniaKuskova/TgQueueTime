using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgQueueTime;

public class OrganizationBot
{
    private static ITelegramBotClient botClient;
    private static ReceiverOptions receiverOptions;
    private static Dictionary<string, string> botResponses;
    private static readonly Dictionary<long, UserState> userStates = new();
    private static readonly Dictionary<long, string> organizationNames = new();
    private static readonly Dictionary<long, string> serviceAverageTimeUpdate = new();
    private static readonly Dictionary<long, Dictionary<string, TimeSpan>> serviceAverageTime = new();

    public static async Task Run()
    {
        botClient = new TelegramBotClient("");
        receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message
            },
            ThrowPendingUpdates = true
        };

        botResponses = new Dictionary<string, string>
        {
            ["/menu"] = $"Список доступных команд:\n" +
                        $"/registration - регистрация новой оганизации\n" +
                        $"/add_service - добавить услугу в вашу оганизацию\n" +
                        $"/get_all_clients - получить список всех клиентов в очереди\n" +
                        $"/change_average_time - изменить среднее время для услуги\n" +
                        $"/menu - вернуться в меню",
            ["/start"] = $"Добро пожаловать в панель управления электронной очередью TgQueueTime!\n" +
                        $"Я бот, который поможет вам эффективно управлять очередями и обслуживать клиентов. " +
                        $"Для более подробной информации, что я умею, введи команду /menu",
            ["/registration"] = "Введите название вашей организации, по которой ваши клиенты вас распознают",
            ["/add_service"] = "Введите название сервиса",
            ["/get_all_clients"] = "Введите номер окна",
            ["/change_average_time"] = "Введите название услуги",
            ["default"] = "Извините, я Вас не понимаю, вот список доступных команд /menu"
        };
        
        using var cts = new CancellationTokenSource();
        botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);
        var me = await botClient.GetMeAsync(cts.Token);
        Console.WriteLine($"{me.FirstName} запущен!");
        await Task.Delay(-1);
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    var message = update.Message;
                    var chat = message.Chat;
                    userStates.TryAdd(chat.Id, UserState.Start);
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

        if (text == "/menu")
        {
            await botClient.SendTextMessageAsync(chat.Id, botResponses[text]);
            userStates[chat.Id] = UserState.Start;
            return;
        }

        switch (state)
        {
            case UserState.Start:
                StartStateHandler(botClient, chat, text);
                break;

            case UserState.WaitingForNameOrganization:
                organizationNames[chat.Id] = text;
                //var responce = RegisterOrganizationCommand(chat.Id, Names[chat.Id]);
                await botClient.SendTextMessageAsync(chat.Id, 
                    "Поздравляю ваша организация успешно зарегистрирована!");
                userStates[chat.Id] = UserState.Start;
                break;


            case UserState.WaitingForNameService:
                serviceAverageTime[chat.Id] = new Dictionary<string, TimeSpan>
                {
                    [text] = new()
                };
                await botClient.SendTextMessageAsync(chat.Id,
                    "Продолжим, введите среднее время обслуживания одного клиента (в минутах). " +
                    "Например: 15 минут");
                userStates[chat.Id] = UserState.WaitingForAverageTime;
                break;

            case UserState.WaitingForAverageTime:
                var part = text.Split(' ');
                if (!int.TryParse(part[0], out var minute))
                {
                    await botClient.SendTextMessageAsync(chat.Id, "Неккоректный ввод. Введите время в минутах");
                    break;
                }

                var serviceName = serviceAverageTime[chat.Id].Keys.First();
                serviceAverageTime[chat.Id][serviceName] = new TimeSpan(0, minute, 0);

                await botClient.SendTextMessageAsync(chat.Id,
                    "Теперь введите список окон, где будет предоставляться эта услуга (через запятую). " +
                    "Например: 1, 2, 3");
                userStates[chat.Id] = UserState.WaitingForNumbersWindow;
                break;
            
            case UserState.WaitingForNumbersWindow:
                var windows = text.Split(',').Select(x => x.Trim()).ToList();
                if (windows.Any(x => !int.TryParse(x, out _)))
                {
                    await botClient.SendTextMessageAsync(chat.Id, "Неккоректный ввод. Введите номера окон через запятую");
                    break;
                }

                var name = serviceAverageTime[chat.Id].Keys.First();
                //var responce = AddService(chat.Id, name, ServiceAverageTime[chat.Id][name],
                //    windows.Select(x => int.Parse(x)));
                await botClient.SendTextMessageAsync(chat.Id, "Услуга успешно добавлена!");
                userStates[chat.Id] = UserState.Start;
                break;
            
            case UserState.WaitingForNameServiceUpdate:
                serviceAverageTimeUpdate[chat.Id] = text;
                await botClient.SendTextMessageAsync(chat.Id,
                    "Отлично! Теперь введите среднее время обслуживания клиента (в минутах). " +
                    "Например: 15 минут");
                userStates[chat.Id] = UserState.WaitingForAverageTimeUpdate;
                break;

            case UserState.WaitingForAverageTimeUpdate:
                var parts = text.Split(' ');
                if (!int.TryParse(parts[0], out var minutes))
                {
                    await botClient.SendTextMessageAsync(chat.Id, "Неккоректный ввод. Введите время в минутах");
                    break;
                }

                userStates[chat.Id] = UserState.Start;
                //var responce = UpdateServiceAverageTimeCommand(chat.Id, ServiceAverageTimeUpdate[chat.Id],
                //    new TimeSpan(0, minutes, 0));
                await botClient.SendTextMessageAsync(chat.Id, "Время успешно обновлено");
                break;

            case UserState.WaitingForNumberWindowGet:
                if (!int.TryParse(text, out var number))
                {
                    await botClient.SendTextMessageAsync(chat.Id, "Неккоректный ввод. Введите номер окна");
                    break;
                }

                userStates[chat.Id] = UserState.Start;
                //var clients = GetAllClientsInQueueQuery(chat.Id, number);
                await botClient.SendTextMessageAsync(chat.Id, "Список клиентов");
                break;
        }
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

    private static async void StartStateHandler(ITelegramBotClient botClient, Chat chat, string text)
    {
        if (!botResponses.TryGetValue(text, out var botResponse))
            botResponse = botResponses["default"];
        await botClient.SendTextMessageAsync(chat.Id, botResponse);
        userStates[chat.Id] = text switch
        {
            "/registration" => UserState.WaitingForNameOrganization,
            "/add_service" => UserState.WaitingForNameService,
            "/get_all_clients" => UserState.WaitingForNumberWindowGet,
            "/change_average_time" => UserState.WaitingForNameServiceUpdate,
            _ => userStates[chat.Id]
        };
    }

    private static bool TryParseAverageTime(string text)
    {
        var parts = text.Split(' ');
        return int.TryParse(parts[0], out _);
    }
}

public enum UserState
{
    Start,
    WaitingForNameOrganization,
    WaitingForNameService,
    WaitingForAverageTime,
    WaitingForNumbersWindow,
    WaitingForAverageTimeUpdate,
    WaitingForNameServiceUpdate,
    WaitingForNumberWindowGet
}