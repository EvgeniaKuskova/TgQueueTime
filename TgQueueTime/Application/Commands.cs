using Domain;
using Domain.Services;

namespace TgQueueTime.Application
{
    public class Commands
    {
        private readonly OrganizationService _organizationService;
        private readonly QueueService _queueService;

        public Commands(OrganizationService organizationService, QueueService queueService)
        {
            _organizationService = organizationService;
            _queueService = queueService;
        }

        public async Task RegisterOrganizationCommand(long idOrganization, string organizationName, int windowCount)
        {
            var organization = new Organization(idOrganization, organizationName, windowCount);
            await _organizationService.RegisterOrganizationAsync(organization);
        }

        public async Task AddClientToQueueCommand(long idClient, string serviceName, string organizationName, int windowNumber)
        {
            var service = new Service(serviceName, TimeSpan.Zero); // здесь нужно достать из бд среднее время услуги
            var client = new Client(idClient, service);
            await _queueService.AddClientToQueueAsync(client, organizationName, windowNumber);
        }
    }
}