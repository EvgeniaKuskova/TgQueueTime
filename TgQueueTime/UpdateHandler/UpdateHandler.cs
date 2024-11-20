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
    private readonly Dictionary<long, UserState> _userStates;
    private readonly Dictionary<string, ICommand> _botResponses;
    private readonly Dictionary<long, string> _serviceAverageTimeUpdate = new();
    private readonly Dictionary<long, Dictionary<string, TimeSpan>> _serviceAverageTime = new();
    private readonly Dictionary<UserState, ICommand> _stateCommands;
    
    public UpdateHandler(ITelegramBotClient botClient)
    {
        _botClient = botClient;
        _userStates = new Dictionary<long, UserState>();
        _botResponses = new Dictionary<string, ICommand>
        {
            ["/menu"] = new BotCommand($"Список доступных команд:\n" +
                                       $"/registration - регистрация новой оганизации\n" +
                                       $"/add_service - добавить услугу в вашу оганизацию\n" +
                                       $"/get_all_clients - получить список всех клиентов в очереди\n" +
                                       $"/change_average_time - изменить среднее время для услуги\n" +
                                       $"/accept_next_client - принять следующего клиента в окне\n" +
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
            ["/accept_next_client"] = new BotCommand("Введите номер окна",
                UserState.WaitingForNumberWindowToAccept),
            ["default"] = new BotCommand("Извините, я Вас не понимаю, вот список доступных команд /menu",
                UserState.Start)
        };

        _stateCommands = new Dictionary<UserState, ICommand>
        {
            [UserState.WaitingForNameOrganization] = new RegisterOrganization(),
            [UserState.WaitingForNameService] = new FixingServiceName(_serviceAverageTime),
            [UserState.WaitingForAverageTime] = new FixingAverageTime(_serviceAverageTime),
            [UserState.WaitingForNumbersWindow] = new RegisterService(_serviceAverageTime),
            [UserState.WaitingForAverageTimeUpdate] = new UpdatingAverageTime(_serviceAverageTimeUpdate),
            [UserState.WaitingForNameServiceUpdate] = new FixingNameServiceUpdate(_serviceAverageTimeUpdate),
            [UserState.WaitingForNumberWindowGet] = new GettingAllClients(),
            [UserState.WaitingForNumberWindowToAccept] = new AcceptingNextClient(),
        };
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            if (!_userStates.ContainsKey(chatId))
            {
                _userStates[chatId] = UserState.Start;
            }

            var userState = _userStates[chatId];
            
            if (messageText == "/menu")
            {
                await _botResponses[messageText].ExecuteAsync(_botClient, chatId, _userStates, messageText);
                return;
            }

            if (userState == UserState.Start)
            {
                if (_botResponses.TryGetValue(messageText, out var command))
                    await command.ExecuteAsync(_botClient, chatId, _userStates, messageText);
                else
                    await _botResponses["default"].ExecuteAsync(_botClient, chatId, _userStates, messageText);
            }

            else
            {
                await _stateCommands[userState].ExecuteAsync(_botClient, chatId, _userStates, messageText);
            }
        }
    }
}