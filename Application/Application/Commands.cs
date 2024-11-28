using Domain;
using Domain.Entities;
using Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace TgQueueTime.Application;

public class Commands
{
    private readonly OrganizationService _organizationService;
    private readonly QueueService _queueService;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<ServiceEntity?> _serviceRepository;
    private readonly IRepository<QueueEntity> _queueRepository;

    public Commands(OrganizationService organizationService, QueueService queueService,
        IRepository<OrganizationEntity> organizationRepository,
        IRepository<ServiceEntity?> serviceRepository,
        IRepository<QueueEntity> queueRepository)
    {
        _organizationService = organizationService;
        _queueService = queueService;
        _organizationRepository = organizationRepository;
        _serviceRepository = serviceRepository;
        _queueRepository = queueRepository;
    }

    public async Task RegisterOrganizationCommand(long idOrganization, string organizationName)
    {
        var organization = new Organization(idOrganization, organizationName);
        await _organizationService.RegisterOrganizationAsync(organization);
    }

    public async Task AddClientToQueueCommand(long idClient, string serviceName, string organizationName)
    {
        var organizationEntity = await _organizationRepository.GetByConditionsAsync(o => o.Name == organizationName);
        if (organizationEntity == null)
        {
            throw new InvalidOperationException($"Организация с именем {organizationName} не найдена.");
        }

        var organization = organizationEntity.ToDomain(_serviceRepository);
        var serviceEntity = await _serviceRepository.GetByConditionsAsync(
            s => s.Name == serviceName && s.OrganizationId == organization.Id);
        if (serviceEntity == null)
        {
            throw new InvalidOperationException(
                $"Услуга '{serviceName}' не найдена в организации '{organizationEntity.Name}'.");
        }
        
        var service = new Service(serviceEntity.Name, TimeSpan.Parse(serviceEntity.AverageTime));
        var client = new Client(idClient, service);
        await _queueService.AddClientToQueueAsync(client, organization);
    }

    public async Task UpdateServiceAverageTimeCommand(long idOrganization, string serviceName, TimeSpan newAverageTime)
    {
        var organizationEntity = await _organizationRepository.GetByIdAsync(idOrganization);
        if (organizationEntity == null)
        {
            throw new InvalidOperationException($"Организация с id {idOrganization} не найдена.");
        }

        var organization = organizationEntity.ToDomain(_serviceRepository);
        
        var serviceEntity = await _serviceRepository.GetByConditionsAsync(
            s => s.Name == serviceName && s.OrganizationId == organization.Id);
        if (serviceEntity == null)
        {
            throw new InvalidOperationException(
                $"Услуга '{serviceName}' не найдена в организации '{organizationEntity.Name}'.");
        }
        
        var service = new Service(serviceEntity.Name, TimeSpan.Parse(serviceEntity.AverageTime));
        
        await _organizationService.UpdateServiceAverageTimeCommandAsunc(organization, service, newAverageTime);
    }

    public async Task AddService(long idOrganization, string serviceName, TimeSpan averageTime, List<int> windowNumbers)
    {
        var organizationEntity = await _organizationRepository.GetByIdAsync(idOrganization);
        if (organizationEntity == null)
        {
            throw new InvalidOperationException($"Организация с id {idOrganization} не найдена.");
        }
        var organization = organizationEntity.ToDomain(_serviceRepository);

        var service = new Service(serviceName, averageTime);


        foreach (var windowNumber in windowNumbers)
        {
            await _organizationService.AddServiceAsync(organization, service, windowNumber);
        }
    }

    public async Task MoveQueue(long idOrganization, int windowNumber)
    {
        var organizationEntity = await _organizationRepository.GetByIdAsync(idOrganization);
        if (organizationEntity == null)
        {
            throw new InvalidOperationException($"Организация с id {idOrganization} не найдена.");
        }

        var organization = organizationEntity.ToDomain(_serviceRepository);
        await _queueService.MoveQueue(organization, windowNumber);
    }
}