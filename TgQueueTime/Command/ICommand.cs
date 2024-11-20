using Telegram.Bot;

namespace TgQueueTime.Command;

public interface ICommand
{
    Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates, string messageText);
}