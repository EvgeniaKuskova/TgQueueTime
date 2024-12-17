﻿using Domain;
using Domain.Entities;
using Domain.Services;

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

    public async Task<TimeSpan> GetClientTimeQuery(long idClient)
    {
        var clientEntity = await _clientRepository.GetByConditionsAsync(client => client.UserId == idClient);
        if (clientEntity == null)
        {
            throw new InvalidOperationException($"Клиент с id {idClient} не стоит в очереди");
        }

        return await _queueService.GetClientTimeQuery(clientEntity);
    }

    public async Task<int> GetNumberClientsBeforeQuery(long idClient)
    {
        var clientEntity = await _clientRepository.GetByConditionsAsync(client => client.UserId == idClient);
        if (clientEntity == null)
        {
            throw new InvalidOperationException($"Клиент с id {idClient} не стоит в очереди");
        }

        return await _queueService.GetNumberClientsBeforeQuery(clientEntity);
    }

    public async Task<List<Client>> GetAllClientsInQueueQuery(long idOrganization, int windowNumber)
    {
        var organizationEntity = await _organizationRepository.GetByConditionsAsync(org => org.Id == idOrganization);
        if (organizationEntity == null)
        {
            throw new InvalidOperationException($"Организация с id {idOrganization} не найдена.");
        }
        var organization = organizationEntity.ToDomain(_serviceRepository);
        return await _queueService.GetAllClientsInQueueQuery(organization, windowNumber);
    }

    public async Task<List<Service>> GetAllServices(string nameOrganization)
    {
        var organizationEntity = await _organizationRepository.GetByConditionsAsync(o => o.Name == nameOrganization);
        if (organizationEntity == null)
        {
            throw new InvalidOperationException($"Организация с id {nameOrganization} не найдена.");
        }
        var organization = organizationEntity.ToDomain(_serviceRepository);
        return await _queueService.GetAllServices(organization);
    }

    public async Task<List<Organization>> GetAllOrganizations()
    {
        return await _organizationService.GetAllOrganizations();
    }

    public async Task<bool> IsQueueStarted(long idOrganization, int windowNumber)
    {
        var organizationEntity = await _organizationRepository.GetByConditionsAsync(org => org.Id == idOrganization);
        if (organizationEntity == null)
        {
            throw new InvalidOperationException($"Организация с id {idOrganization} не найдена.");
        }
        var organization = organizationEntity.ToDomain(_serviceRepository);
        return await _queueService.IsQueueStarted(organization, windowNumber);
    }
}