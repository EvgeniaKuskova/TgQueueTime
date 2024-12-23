﻿using Telegram.Bot;

namespace TelegramBots.Command;

public class FixingAverageTime: ICommand
{
    private readonly Dictionary<long, Dictionary<string, TimeSpan>> _serviceAverageTime;
    private readonly string _goodresponse;

    public FixingAverageTime(Dictionary<long, Dictionary<string, TimeSpan>> serviceAverageTime)
    {
        _serviceAverageTime = serviceAverageTime;
        _goodresponse = "Теперь введите список окон, где будет предоставляться эта услуга (через запятую). " +
                    "Например: 1, 2, 3";
    }
    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText, CancellationToken cancellationToken)
    {
        var parts = messageText.Split(' ');
        if (!int.TryParse(parts[0], out var minute) || minute < 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите время в минутах");
            return;
        }
        var serviceName = _serviceAverageTime[chatId].Keys.First();
        _serviceAverageTime[chatId][serviceName] = new TimeSpan(0, minute, 0);
        await botClient.SendTextMessageAsync(chatId, _goodresponse);
        userStates[chatId] = UserState.WaitingForNumbersWindow;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingForAverageTime;
    }
}