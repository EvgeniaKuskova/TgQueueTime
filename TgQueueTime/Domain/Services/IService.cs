namespace Domain.Services
{
    public interface IQueueService
    {
        Task AddClientToQueueAsync(Client client, string organizationName, int windowNumber);
        Task CreateQueueAsync(Organization organization, int windowNumber);
        // Другие методы управления очередью
    }

    public interface IOrganizationService
    {
        Task RegisterOrganizationAsync(Organization organization);
        // Другие методы управления организацией
    }
}