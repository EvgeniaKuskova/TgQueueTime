using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgQueueTime.Command;
using BotCommand = TgQueueTime.Command.BotCommand;
using ICommand = TgQueueTime.Command.ICommand;

namespace TgQueueTime;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly Dictionary<long, UserState> userStates;
    private readonly Dictionary<string, ICommand> botResponses;
    private static readonly Dictionary<long, string> serviceAverageTimeUpdate = new();
    private static readonly Dictionary<long, Dictionary<string, TimeSpan>> serviceAverageTime = new();
    
    public UpdateHandler(ITelegramBotClient botClient)
    {
        _botClient = botClient;
        userStates = new Dictionary<long, UserState>();
        botResponses = new Dictionary<string, ICommand>
        {
            ["/menu"] = new BotCommand($"Список доступных команд:\n" +
                                       $"/registration - регистрация новой оганизации\n" +
                                       $"/add_service - добавить услугу в вашу оганизацию\n" +
                                       $"/get_all_clients - получить список всех клиентов в очереди\n" +
                                       $"/change_average_time - изменить среднее время для услуги\n" +
                                       $"/menu - вернуться в меню",
                UserState.Start),
            ["/start"] = new BotCommand($"Добро пожаловать в панель управления электронной очередью TgQueueTime!\n" +
                                        $"Я бот, который поможет вам эффективно управлять очередями и обслуживать клиентов. " +
                                        $"Для более подробной информации, что я умею, введи команду /menu",
                UserState.Start),
            ["/registration"] = new BotCommand("Введите название вашей организации, по которой ваши клиенты вас распознают",
                UserState.WaitingForNameOrganization),
            ["/add_service"] = new BotCommand("Введите название услуги, которую хотите добавить в вашу организацию",
                UserState.WaitingForNameService),
            ["/get_all_clients"] = new BotCommand("Введите номер окна",
                UserState.WaitingForNumberWindowGet),
            ["/change_average_time"] = new BotCommand("Введите название услуги",
                UserState.WaitingForNameServiceUpdate),
            ["default"] = new BotCommand("Извините, я Вас не понимаю, вот список доступных команд /menu",
                UserState.Start)
        };
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            if (!userStates.ContainsKey(chatId))
            {
                userStates[chatId] = UserState.Start;
            }

            var userState = userStates[chatId];
            
            if (messageText == "/menu")
            {
                await botResponses[messageText].ExecuteAsync(_botClient, chatId, userStates);
                return;
            }
            
            
            switch (userState)
            {
                case UserState.WaitingForNameOrganization:
                    await new RegisterOrganization().ExecuteAsync(_botClient, chatId, userStates);
                    return;
                
                case UserState.WaitingForNameService:
                    await new FixingServiceName(serviceAverageTime, messageText).ExecuteAsync(_botClient, chatId,
                        userStates);
                    break;

                case UserState.WaitingForAverageTime:
                    await new FixingAverageTime(serviceAverageTime, messageText).ExecuteAsync(_botClient, chatId,
                        userStates);
                    break;
                
                case UserState.WaitingForNumbersWindow:
                    await new RegisterService(serviceAverageTime, messageText).ExecuteAsync(_botClient, chatId,
                        userStates);
                    break;
                
                case UserState.WaitingForAverageTimeUpdate:
                    await new UpdatingAverageTime(serviceAverageTimeUpdate, messageText).ExecuteAsync(_botClient, chatId,
                        userStates);
                    break;
                
                case UserState.WaitingForNameServiceUpdate:
                    await new FixingNameServiceUpdate(serviceAverageTimeUpdate, messageText).ExecuteAsync(_botClient, chatId,
                        userStates);
                    break;
                
                case UserState.WaitingForNumberWindowGet:
                    await new GettingAllClients(messageText).ExecuteAsync(_botClient, chatId,
                        userStates);
                    break;
                
                default:
                    if (botResponses.TryGetValue(messageText, out var command))
                        await command.ExecuteAsync(_botClient, chatId, userStates);
                    else
                        await botResponses["default"].ExecuteAsync(_botClient, chatId, userStates);
                    return;
            }
        }
    }
}