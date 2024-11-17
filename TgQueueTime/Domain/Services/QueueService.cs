using Domain.Entities;
using Infrastructure.Repositories;

namespace Domain.Services
{
    public class QueueService : IQueueService
    {
        private readonly IRepository<QueueEntity> _queueRepository;
        private readonly IRepository<QueueClientsEntity> _clientRepository;
        private readonly IRepository<OrganizationEntity> _organizationRepository;

        public QueueService(
            IRepository<QueueEntity> queueRepository,
            IRepository<QueueClientsEntity> clientRepository,
            IRepository<OrganizationEntity> organizationRepository)
        {
            _queueRepository = queueRepository;
            _clientRepository = clientRepository;
            _organizationRepository = organizationRepository;
        }

        public async Task AddClientToQueueAsync(Client client, string organizationName, int windowNumber)
        {
            // добавить клиента в очередь в бд
        }

        public async Task CreateQueueAsync(Organization organization, int windowNumber)
        {
            // добавить очередь в базу данных
        }
    }

    public class OrganizationService : IOrganizationService
    {
        private readonly IRepository<OrganizationEntity> _organizationRepository;

        public OrganizationService(IRepository<OrganizationEntity> organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }

        public async Task RegisterOrganizationAsync(Organization organization)
        {
            var entity = new OrganizationEntity().FromDomain(organization);
            await _organizationRepository.AddAsync(entity);
        }
    }
}