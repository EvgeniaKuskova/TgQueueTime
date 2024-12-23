﻿using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBots;

public interface IUpdateHandler
{
    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}