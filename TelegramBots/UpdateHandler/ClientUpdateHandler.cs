using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBots.Command;
using BotCommand = TelegramBots.Command.BotCommand;
using ICommand = TelegramBots.Command.ICommand;

namespace TelegramBots;

public class ClientUpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly Dictionary<long, UserState> _userStates;
    private readonly Dictionary<string, ICommand> _botResponses;
    private readonly Dictionary<long, string> _organization = new();
    private readonly ICommand[] _commands;

    public ClientUpdateHandler(ITelegramBotClient botClient)
    {
        _botClient = botClient;
        _userStates = new Dictionary<long, UserState>();
        _botResponses = new Dictionary<string, ICommand>
        {
            ["/menu"] = new BotCommand($"Список доступных команд:\n" +
                                       $"/register - регистрация в очередь\n" +
                                       $"/mytime - узнать время ожидания\n" +
                                       $"/clientsbeforeme - количество клиентов до вас\n" +
                                       $"/menu - список команд",
                UserState.Start),
            ["/start"] = new BotCommand($"Вы зашли в бот, где можно зарегистрироваться в очередь и отслеживать время до получения услуги.\n" +
                                        $"Для того, чтобы посмотреть команды, введите команду /menu",
                UserState.ClientStart),
            ["/register"] = new BotCommand("Введите название организации, в которой хотите попасть в очередь",
                UserState.WaitingClientForNameOrganization),
            ["/mytime"] = new TakeMyTime(),
            ["/clientsbeforeme"] = new ClientBefore(),
            ["default"] = new BotCommand("Извините, я Вас не понимаю, вот список доступных команд /menu",
                UserState.ClientStart)
        };

        _commands = new ICommand[]
        {
            new RegisterClient(_organization),
            new GetNameService(_organization)
        };
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            if (!_userStates.ContainsKey(chatId))
                _userStates[chatId] = UserState.ClientStart;

            var userState = _userStates[chatId];

            if (_botResponses.TryGetValue(messageText, out var command))
                await command.ExecuteAsync(_botClient, chatId, _userStates, messageText);
            else if (userState == UserState.ClientStart)
                await _botResponses["default"].ExecuteAsync(_botClient, chatId, _userStates, messageText);
            else
            {
                await _commands
                    .First(x => x.Accept(userState))
                    .ExecuteAsync(_botClient, chatId, _userStates, messageText);
            }
        }
    }
}

