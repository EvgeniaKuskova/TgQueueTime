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
        try
        {
            var taskGetAllOrganizations = _queries.GetAllOrganizations();
            var allOrganizations = taskGetAllOrganizations.Result;
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
            var allServices = taskGetAllServices.Result;
            var nameServices = allServices.Select(x => x.Name).ToArray();
            string servicesString = string.Join(" | ", nameServices);

            await botClient.SendTextMessageAsync(chatId, $"Выбери услугу, где нужно занять очередь ( | это разделитель): {servicesString}");
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
            if (e is InvalidOperationException)
                await botClient.SendTextMessageAsync(chatId, e.Message);
            throw;
        }

        userStates[chatId] = UserState.WaitingClientForNameService;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForNameOrganization;
    }
}
