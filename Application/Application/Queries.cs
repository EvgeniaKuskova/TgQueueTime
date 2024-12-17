using Domain;
using Domain.Entities;
using Domain.Services;
using CSharpFunctionalExtensions;
namespace TgQueueTime.Application;

public class Queries
{
    private readonly OrganizationService _organizationService;
    private readonly QueueService _queueService;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<ServiceEntity?> _serviceRepository;
    private readonly IRepository<QueueEntity> _queueRepository;
    private readonly IRepository<ClientsEntity> _clientRepository;
    private readonly IRepository<QueueServicesEntity> _queueServicesRepository;

    public Queries(OrganizationService organizationService, QueueService queueService,
        IRepository<OrganizationEntity> organizationRepository,
        IRepository<ServiceEntity?> serviceRepository,
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

    public async Task<Result<TimeSpan>> GetClientTimeQuery(long idClient)
    {
        var clientEntity = await _clientRepository.GetByConditionsAsync(client => client.UserId == idClient);
        if (clientEntity == null)
        {
            return Result.Failure<TimeSpan>($"Клиент с id {idClient} не стоит в очереди");
        }

        var timeQueryResult =  await _queueService.GetClientTimeQuery(clientEntity);
        return Result.Success(timeQueryResult);
    }

    public async Task<Result<int>> GetNumberClientsBeforeQuery(long idClient)
    {
        var clientEntity = await _clientRepository.GetByConditionsAsync(client => client.UserId == idClient);
        if (clientEntity == null)
        {
            return Result.Failure<int>($"Клиент с id {idClient} не стоит в очереди");
        }
        var clients = await _queueService.GetNumberClientsBeforeQuery(clientEntity);
        return Result.Success(clients);
    }

    public async Task<Result<List<Client>>> GetAllClientsInQueueQuery(long idOrganization, int windowNumber)
    {
        var organizationEntity = await _organizationRepository.GetByConditionsAsync(org => org.Id == idOrganization);
        if (organizationEntity == null)
        {
            return Result.Failure<List<Client>>($"Организация с id {idOrganization} не найдена.");
        }
        var organization = organizationEntity.ToDomain(_serviceRepository);
        var clients = await _queueService.GetAllClientsInQueueQuery(organization, windowNumber);
        return clients;
    }

    public async Task<Result<List<Service>>> GetAllServices(string nameOrganization)
    {
        var organizationEntity = await _organizationRepository.GetByConditionsAsync(o => o.Name == nameOrganization);
        if (organizationEntity == null)
        {
            return Result.Failure<List<Service>>("Организация с именем {nameOrganization} не найдена.");
        }
        var organization = organizationEntity.ToDomain(_serviceRepository);
        var services = await _queueService.GetAllServices(organization);
        return Result.Success(services);
    }

    public async Task<List<Organization>> GetAllOrganizations()
    {
        return await _organizationService.GetAllOrganizations();
    }

    public async Task<Result<bool>> IsQueueStarted(string nameOrganization, long idClient)
    {
        var organizationEntity = await _organizationRepository.GetByConditionsAsync(o => o.Name == nameOrganization);
        if (organizationEntity == null)
        {
            return Result.Failure<bool>("Ваша организация не зарегистрирована");
        }
        var organization = organizationEntity.ToDomain(_serviceRepository);
        var clientEntity = await _clientRepository.GetByConditionsAsync(client => client.UserId == idClient);
        if (clientEntity == null)
        {
            return Result.Failure<bool>($"Клиент с id {idClient} не стоит в очереди");
        }

        return await _queueService.IsQueueStarted(organization, clientEntity);
    }
}