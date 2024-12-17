using Domain;
using Telegram.Bot;
using TgQueueTime.Application;

namespace TelegramBots.Command;

public class GetNameOrganization : ICommand
{
    private readonly Queries _queries;
    private List<Organization> _allOrganizations;

    public GetNameOrganization(Queries queries)
    {
        _queries = queries;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, long chatId, Dictionary<long, UserState> userStates,
        string messageText, CancellationToken cancellationToken)
    {
        try
        {
            var task = _queries.GetAllOrganizations();
            _allOrganizations = await task;
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
            if (e is InvalidOperationException)
                await botClient.SendTextMessageAsync(chatId, e.Message);
            throw;
        }
        var nameOrganizations = _allOrganizations.Select(x => x.Name).ToArray();
        string organizationsString = string.Join(" | ", nameOrganizations);

        await botClient.SendTextMessageAsync(chatId, $"Выбери организацию, в которой нужно занять очередь ( | это разделитель): {organizationsString}");
        userStates[chatId] = UserState.WaitingClientForNameOrganization;
    }

    public bool Accept(UserState userState)
    {
        return userState == UserState.WaitingClientForGetNameOrganization;
    }
}
