using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IRepository<QueueEntity> _queueRepository;
    private readonly IRepository<QueueServicesEntity> _queueServicesRepository;
    private readonly IRepository<ClientsEntity> _clientRepository;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<ServiceEntity> _serviceRepository;

    public OrganizationService(IRepository<QueueEntity> queueRepository,
        IRepository<QueueServicesEntity> queueServicesRepository,
        IRepository<ClientsEntity> clientRepository,
        IRepository<OrganizationEntity> organizationRepository,
        IRepository<ServiceEntity> serviceRepository)
    {
        _queueRepository = queueRepository;
        _clientRepository = clientRepository;
        _organizationRepository = organizationRepository;
        _queueServicesRepository = queueServicesRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task RegisterOrganizationAsync(Organization organization)
    {
        var entity = new OrganizationEntity().FromDomain(organization);
        await _organizationRepository.AddAsync(entity);
    }

    public async Task UpdateServiceAverageTimeCommandAsunc(Organization organization, Service service,
        TimeSpan newAverageTime)
    {
        var serviceEntity = await _serviceRepository.GetByConditionsAsync(s =>
            s.Name == service.Name && s.OrganizationId == organization.Id);

        serviceEntity.AverageTime = newAverageTime.ToString();
        await _serviceRepository.UpdateAsync(serviceEntity);
    }

    public async Task AddServiceAsync(Organization organization, Service service, int windowNumber)
    {
        var existingServiceEntity = await _serviceRepository.GetByConditionsAsync(
            s => s.Name == service.Name && s.OrganizationId == organization.Id);

        if (existingServiceEntity == null)
        {
            var serviceEntity = new ServiceEntity
            {
                Name = service.Name,
                AverageTime = service.AverageTime.ToString(),
                OrganizationId = organization.Id
            };

            await _serviceRepository.AddAsync(serviceEntity);
            existingServiceEntity = serviceEntity;
        }

        var queueEntity = await _queueRepository.GetByConditionsAsync(
            q => q.OrganizationId == organization.Id && q.WindowNumber == windowNumber);

        var existingQueueService = await _queueServicesRepository.GetByConditionsAsync(
            qs => qs.QueueId == queueEntity.Id && qs.ServiceId == existingServiceEntity.Id);

        if (existingQueueService == null)
        {
            var queueServiceEntity = new QueueServicesEntity
            {
                QueueId = queueEntity.Id,
                ServiceId = existingServiceEntity.Id
            };

            await _queueServicesRepository.AddAsync(queueServiceEntity);
        }
    }

    public async Task<List<Organization>> GetAllOrganizations()
    {
        var organizationEntities = await _organizationRepository
            .GetAllByCondition(_ => true)
            .ToListAsync();
        var organizations = organizationEntities
            .Select(entity => entity.ToDomain(_serviceRepository))
            .ToList();

        return organizations;
    }


}