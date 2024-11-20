using Domain.Entities;
using Domain.Services;
using Infrastructure.Repositories;

namespace TgQueueTime.Application;

public class Queries
{
    private readonly OrganizationService _organizationService;
    private readonly QueueService _queueService;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<ServiceEntity> _serviceRepository;
    private readonly IRepository<QueueEntity> _queueRepository;
    private readonly IRepository<ClientsEntity> _clientRepository;
    private readonly IRepository<QueueServicesEntity> _queueServicesRepository;

    public Queries(OrganizationService organizationService, QueueService queueService,
        IRepository<OrganizationEntity> organizationRepository,
        IRepository<ServiceEntity> serviceRepository,
        IRepository<QueueEntity> queueRepository,
        IRepository<ClientsEntity> clientRepository,
        IRepository<QueueServicesEntity> queueServicesRepository)
    {
        _organizationService = organizationService;
        _queueService = queueService;
        _organizationRepository = organizationRepository;
        _serviceRepository = serviceRepository;
        _queueRepository = queueRepository;
        _clientRepository = clientRepository;
        _queueServicesRepository = queueServicesRepository;
    }

    async Task<TimeSpan> GetClientTimeQuery(long idClient)
    {
        var clientEntity = await _clientRepository.GetByIdAsync(idClient);
        if (clientEntity == null)
        {
            throw new InvalidOperationException($"Клиент с id {idClient} не стоит в очереди");
        }

        return await _queueService.GetClientTimeQuery(clientEntity);
    }

    void GetNumberClientsBeforeQuery(long idClient, string organizationName)
    {
        // заглушка
    }

    void GetAllClientsInQueueQuery(long idOrganization, int windowNumber)
    {
        // заглушка
    }

    void GetAllServices(string organizationName)
    {
        // заглушка
    }

    void GetAllOrganizations()
    {
        // заглушка
    }
}