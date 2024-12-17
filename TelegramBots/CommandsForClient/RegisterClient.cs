using Domain;
using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class RegisterClient : ICommand
{
    private readonly Dictionary<long, string> _organization;
    private readonly Queries _queries;

    public RegisterClient(Dictionary<long, string> organization, Queries queries)
    {
        _organization = organization;
        _queries = queries;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText, CancellationToken cancellationToken)
    {
        var allOrganizations = await _queries.GetAllOrganizations();
        var nameOrganizations = allOrganizations.Select(x => x.Name).ToArray();
        if (Array.Exists(nameOrganizations, name => name == messageText))
            _organization[chatId] = messageText;
        else
        {
            await botClient.SendTextMessageAsync(chatId, "Такой организации не существует, повторите ввод");
            userStates[chatId] = UserState.WaitingClientForNameOrganization;
            return;
        }

        var taskGetAllServices = _queries.GetAllServices(_organization[chatId]);
        var allServices = await _queries.GetAllServices(_organization[chatId]);
        if (allServices.IsFailure)
        {
            await botClient.SendTextMessageAsync(chatId, allServices.Error);
            return;
        }
        
        var nameServices = allServices.Value.Select(x => x.Name).ToArray();
        var servicesString = string.Join(" | ", nameServices);
        await botClient.SendTextMessageAsync(chatId, $"Выбери услугу, где нужно занять очередь ( | это разделитель): {servicesString}");
        userStates[chatId] = UserState.WaitingClientForNameService;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForNameOrganization;
    }
}
