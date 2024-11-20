﻿using Telegram.Bot;

namespace TgQueueTime.Command;

public class RegisterService: ICommand
{
    private readonly Dictionary<long, Dictionary<string, TimeSpan>> _serviceAverageTime;
    private readonly string _goodResponse;

    public RegisterService(Dictionary<long, Dictionary<string, TimeSpan>> serviceAverageTime)
    {
        _serviceAverageTime = serviceAverageTime;
        _goodResponse = "Услуга успешно добавлена!";
    }
    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText)
    {
        var windows = messageText.Split(',').Select(x => x.Trim()).ToList();
        if (windows.Any(x => !int.TryParse(x, out _)))
        {
            await botClient.SendTextMessageAsync(chatId, "Неккоректный ввод. Введите номера окон через запятую");
            return;
        }
        var name = _serviceAverageTime[chatId].Keys.First();
        //var responce = AddService(chat.Id, name, ServiceAverageTime[chat.Id][name],
        //    windows.Select(x => int.Parse(x)));
        await botClient.SendTextMessageAsync(chatId, _goodResponse);
        _serviceAverageTime.Remove(chatId);
        userStates[chatId] = UserState.Start;
    }
}