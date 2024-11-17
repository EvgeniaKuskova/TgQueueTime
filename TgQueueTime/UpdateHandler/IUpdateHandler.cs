using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgQueueTime;

public interface IUpdateHandler
{
    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}