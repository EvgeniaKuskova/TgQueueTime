using Telegram.Bot;

namespace TelegramBots.Command;

public interface ICommand
{
    Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, string messageText);

    bool Accept(UserState userState);
}