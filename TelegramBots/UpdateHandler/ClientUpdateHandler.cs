using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBots.Command;
using TgQueueTime.Application;
using BotCommand = TelegramBots.Command.BotCommand;
using ICommand = TelegramBots.Command.ICommand;

namespace TelegramBots;

public class ClientUpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly Dictionary<long, UserState> _userStates;
    private readonly Dictionary<string, ICommand> _botResponses;
    private readonly Dictionary<long, string> _organization = new();
    private readonly ICommand[] _commandsBot;
    private readonly Commands _commands;
    private readonly Queries _queries;

    public ClientUpdateHandler(ITelegramBotClient botClient, Commands commands, Queries queries)
    {
        _botClient = botClient;
        _commands = commands;
        _queries = queries;
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
            ["/register"] = new GetNameOrganization(_queries),
            ["/mytime"] = new TakeMyTime(_queries),
            ["/clientsbeforeme"] = new ClientBefore(_queries),
            ["default"] = new BotCommand("Извините, я Вас не понимаю, вот список доступных команд /menu",
                UserState.ClientStart)
        };

        _commandsBot = new ICommand[]
        {
            new RegisterClient(_organization, _queries),
            new GetNameService(_organization, _commands, _queries)
        };
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var chatId = update.Message.Chat.Id;
        try
        {
            if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
            {
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
                    await _commandsBot
                        .First(x => x.Accept(userState))
                        .ExecuteAsync(_botClient, chatId, _userStates, messageText);
                }
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

