using Domain;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Repositories;

namespace TgQueueTime.Application;

public class Commands
{
    private readonly OrganizationService _organizationService;
    private readonly QueueService _queueService;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<ServiceEntity> _serviceRepository;

    public Commands(OrganizationService organizationService, QueueService queueService,
        IRepository<OrganizationEntity> organizationRepository,
        IRepository<ServiceEntity> serviceRepository)
    {
        _organizationService = organizationService;
        _queueService = queueService;
        _organizationRepository = organizationRepository;
        _serviceRepository = serviceRepository;
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
}