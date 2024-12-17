using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBots.Command;
using TgQueueTime.Application;
using BotCommand = TelegramBots.Command.BotCommand;
using ICommand = TelegramBots.Command.ICommand;

namespace TelegramBots;

public class OrganizationUpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly Dictionary<long, UserState> _userStates;
    private readonly Dictionary<string, ICommand> _botResponses;
    private readonly Dictionary<long, string> _serviceAverageTimeUpdate = new();
    private readonly Dictionary<long, Dictionary<string, TimeSpan>> _serviceAverageTime = new();
    private readonly ICommand[] _botCommands;
    private readonly Commands _commands;
    private readonly Queries _queries;
    
    public OrganizationUpdateHandler(ITelegramBotClient botClient, Commands commands, Queries queries)
    {
        _botClient = botClient;
        _commands = commands;
        _queries = queries;
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

        _botCommands = new ICommand[]
        {
            new RegisterOrganization(_commands),
            new FixingNameService(_serviceAverageTime),
            new FixingAverageTime(_serviceAverageTime),
            new RegisterService(_serviceAverageTime, _commands),
            new UpdatingAverageTime(_serviceAverageTimeUpdate, _commands),
            new FixingNameServiceUpdate(_serviceAverageTimeUpdate),
            new GettingAllClients(_queries),
            new AcceptingNextClient(_commands),
        };
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var chatId = update.Message.Chat.Id;
        try
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            if (!_userStates.ContainsKey(chatId))
                _userStates[chatId] = UserState.Start;

            var userState = _userStates[chatId];
            
            if (_botResponses.TryGetValue(messageText, out var command))
                await command.ExecuteAsync(_botClient, chatId, _userStates, messageText, cancellationToken);
            else if (userState == UserState.Start)
                await _botResponses["default"].ExecuteAsync(_botClient, chatId, _userStates, messageText, cancellationToken);
            else
            {
                await _botCommands
                    .First(x => x.Accept(userState))
                    .ExecuteAsync(_botClient, chatId, _userStates, messageText, cancellationToken);
            }
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
            await botClient.SendTextMessageAsync(chatId, 
                "Извините, что-то пошло не так. Попробуйте позже.");
        }
    }
}