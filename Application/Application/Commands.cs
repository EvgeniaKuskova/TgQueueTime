using Domain;
using Domain.Entities;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using CSharpFunctionalExtensions;

namespace TgQueueTime.Application;

public class Commands
{
    private readonly OrganizationService _organizationService;
    private readonly QueueService _queueService;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<ServiceEntity?> _serviceRepository;
    private readonly IRepository<QueueEntity> _queueRepository;
    private readonly IRepository<ClientsEntity> _clientRepository;

    public Commands(OrganizationService organizationService,
        QueueService queueService,
        IRepository<OrganizationEntity> organizationRepository,
        IRepository<ServiceEntity?> serviceRepository,
        IRepository<QueueEntity> queueRepository,
        IRepository<ClientsEntity> clientRepository)
    {
        _organizationService = organizationService;
        _queueService = queueService;
        _organizationRepository = organizationRepository;
        _serviceRepository = serviceRepository;
        _queueRepository = queueRepository;
        _clientRepository = clientRepository;
    }

    public async Task<Result> RegisterOrganizationCommand(long idOrganization, string organizationName)
    {
        try
        {
            var existingOrganization = await _organizationRepository.GetByConditionsAsync(o => o.Id == idOrganization);
            if (existingOrganization != null)
                return Result.Failure("Организация уже зарегистрирована на этом аккаунте");
            var organization = new Organization(idOrganization, organizationName);
            await _organizationService.RegisterOrganizationAsync(organization);
        }
        catch (Exception ex)
        {
            return Result.Failure("Произошла ошибка при регистрации организации. Попробуйте позже");
        }

        return Result.Success();
    }

    public async Task<Result> AddClientToQueueCommand(long idClient, string serviceName, string organizationName)
    {
        var clients = await _clientRepository.GetByConditionsAsync(c => c.UserId == idClient);
        if (clients != null)
        {
            return Result.Failure("Вы уже регистрировались ранее");
        }

        var organizationEntity = await _organizationRepository.GetByConditionsAsync(o => o.Name == organizationName);
        if (organizationEntity == null)
        {
            return Result.Failure($"Организация с именем {organizationName} не найдена.");
        }

        var organization = organizationEntity.ToDomain(_serviceRepository);
        var serviceEntity = await _serviceRepository.GetByConditionsAsync(
            s => s.Name == serviceName && s.OrganizationId == organization.Id);
        if (serviceEntity == null)
        {
            return Result.Failure($"Услуга '{serviceName}' не найдена в организации '{organizationEntity.Name}'.");
        }

        var service = new Service(serviceEntity.Name, TimeSpan.Parse(serviceEntity.AverageTime));
        var client = new Client(idClient, service);
        await _queueService.AddClientToQueueAsync(client, organization);
        return Result.Success();
    }

    public async Task<Result> UpdateServiceAverageTimeCommand(long idOrganization, string serviceName,
        TimeSpan newAverageTime)
    {
        var organizationEntity = await _organizationRepository.GetByKeyAsync(idOrganization);
        if (organizationEntity == null)
        {
            return Result.Failure("Ваша организация не зарегистрирована");
        }

        var organization = organizationEntity.ToDomain(_serviceRepository);

        var serviceEntity = await _serviceRepository.GetByConditionsAsync(
            s => s.Name == serviceName && s.OrganizationId == organization.Id);
        if (serviceEntity == null)
        {
            return Result.Failure($"Услуга '{serviceName}' не найдена в организации '{organizationEntity.Name}'.");
        }

        var service = new Service(serviceEntity.Name, TimeSpan.Parse(serviceEntity.AverageTime));
        await _organizationService.UpdateServiceAverageTimeCommandAsunc(organization, service, newAverageTime);
        return Result.Success();
    }

    public async Task<Result> AddService(long idOrganization, string serviceName, TimeSpan averageTime,
        List<int> windowNumbers)
    {
        var organizationEntity = await _organizationRepository.GetByKeyAsync(idOrganization);
        if (organizationEntity == null)
        {
            return Result.Failure("Ваша организация не зарегистрирована.");
        }

        var organization = organizationEntity.ToDomain(_serviceRepository);

        var service = new Service(serviceName, averageTime);

        foreach (var windowNumber in windowNumbers)
        {
            await _organizationService.AddServiceAsync(organization, service, windowNumber);
        }

        return Result.Success();
    }

    public async Task<Result> MoveQueue(long idOrganization, int windowNumber)
    {
        var organizationEntity = await _organizationRepository.GetByKeyAsync(idOrganization);
        if (organizationEntity == null)
        {
            return Result.Failure("Ваша организация не зарегистрирована");
        }

        var organization = organizationEntity.ToDomain(_serviceRepository);
        var result = await _queueService.MoveQueue(organization, windowNumber);
        return result;
    }
}
