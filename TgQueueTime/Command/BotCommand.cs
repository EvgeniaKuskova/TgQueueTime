using Telegram.Bot;

namespace TgQueueTime.Command;

public class BotCommand : ICommand
{
    private readonly string _response;
    private readonly UserState _nextState;
    
    public BotCommand(string response, UserState nextState)
    {
        _response = response;
        _nextState = nextState;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, 
        string messageText)
    {
        await botClient.SendTextMessageAsync(chatId, _response);
        userStates[chatId] = _nextState;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.Start;
    }
}