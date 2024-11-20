using Domain;
using Domain.Entities;
using Domain.Services;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services;

public class QueueService : IQueueService
{
    private readonly IRepository<QueueEntity> _queueRepository;
    private readonly IRepository<QueueServicesEntity> _queueServicesRepository;
    private readonly IRepository<ClientsEntity> _clientRepository;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<ServiceEntity> _serviceRepository;

    public QueueService(
        IRepository<QueueEntity> queueRepository,
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

    public async Task AddClientToQueueAsync(Client client, Organization organization)
    {
        // Найти ServiceEntity по имени услуги и ID организации
        var serviceEntity = await _serviceRepository.GetByConditionsAsync(
            s => s.Name == client.Service.Name && s.OrganizationId == organization.Id);

        if (serviceEntity == null)
        {
            throw new InvalidOperationException(
                $"Услуга '{client.Service.Name}' не найдена в организации '{organization.Name}'.");
        }

        // Получаем все QueueServicesEntity, связанные с найденной услугой
        var queueServices = await _queueServicesRepository
            .GetAllByCondition(qs => qs.ServiceId == serviceEntity.Id)
            .ToListAsync();


        if (!queueServices.Any())
        {
            throw new InvalidOperationException(
                $"Нет очередей, предоставляющих услугу '{client.Service.Name}' в организации '{organization.Name}'.");
        }

        // Получаем все очереди для данной организации
        var queues = await _queueRepository
            .GetAllByValueAsync(q => q.OrganizationId, organization.Id)
            .ToListAsync();

        if (!queues.Any())
        {
            throw new InvalidOperationException($"Для организации с ID {organization.Id} не найдено очередей.");
        }

        // Фильтруем очереди, которые предоставляют услугу клиента
        var relevantQueues = queues.Where(q => queueServices.Any(qs => qs.QueueId == q.Id)).ToList();

        if (!relevantQueues.Any())
        {
            throw new InvalidOperationException(
                $"Нет очередей, предоставляющих услугу '{client.Service.Name}' в организации '{organization.Name}'.");
        }

        // Вычисляем оптимальную очередь
        QueueEntity optimalQueue = null;
        DateTime optimalStartTime = DateTime.MaxValue;

        foreach (var queue in relevantQueues)
        {
            // Получаем всех клиентов в очереди
            var clientsInQueue = await _clientRepository
                .GetAllByValueAsync(c => c.QueueId, queue.Id)
                .ToListAsync();

            // Находим последнего клиента, который начал обслуживание
            var lastStartedClient = clientsInQueue
                .Where(c => !string.IsNullOrEmpty(c.StartTime))
                .OrderByDescending(c => c.Position)
                .FirstOrDefault();

            DateTime queueStartTime;

            if (lastStartedClient != null)
            {
                // Клиент уже обслуживается: его StartTime + AverageTime
                var lastServiceId = queueServices.First(qs => qs.QueueId == queue.Id).ServiceId;
                var lastService = await _serviceRepository.GetByIdAsync(lastServiceId);

                // Парсим StartTime в TimeSpan
                var startTime = DateTime.Parse(lastStartedClient.StartTime);
                queueStartTime = startTime + TimeSpan.Parse(lastService.AverageTime);
            }
            else
            {
                // Очередь пустая или никто не начал обслуживание
                queueStartTime = DateTime.Now;
            }

            // Прибавляем время для всех остальных клиентов в очереди
            foreach (var clientInQueue in clientsInQueue.Where(c => c.Position > (lastStartedClient?.Position ?? 0)))
            {
                var clientServiceId = queueServices.First(qs => qs.QueueId == queue.Id).ServiceId;
                var clientService = await _serviceRepository.GetByIdAsync(clientServiceId);

                queueStartTime += TimeSpan.Parse(clientService.AverageTime);
            }

            // Проверяем, является ли эта очередь более оптимальной
            if (queueStartTime < optimalStartTime)
            {
                optimalStartTime = queueStartTime;
                optimalQueue = queue;
            }


            if (optimalQueue == null)
            {
                throw new InvalidOperationException(
                    $"Не удалось найти оптимальную очередь для клиента в организации '{organization.Name}'.");
            }

            // Добавляем клиента в оптимальную очередь
            var clientEntity = new ClientsEntity
            {
                QueueId = optimalQueue.Id,
                UserId = client.Id,
                Position = await _clientRepository
                    .GetAllByValueAsync(c => c.QueueId, optimalQueue.Id)
                    .CountAsync() + 1,
                StartTime = null,
                QueueServiceId =
                    queueServices.First(qs => qs.QueueId == optimalQueue.Id)
                        .Id // Устанавливаем существующий QueueServiceId
            };


            await _clientRepository.AddAsync(clientEntity);
        }
    }

    public Task CreateQueueAsync(Organization organization, int windowNumber)
    {
        throw new NotImplementedException(); //!!!!!!!!!!!!!!!!
    }

    public async Task<TimeSpan> GetClientTimeQuery(ClientsEntity client)
    {
        var queueServiceEntity = await _queueServicesRepository.GetByIdAsync(client.QueueServiceId);
        var queueId = queueServiceEntity.QueueId;

        var clientsInQueue = await _clientRepository
            .GetAllByValueAsync(c => c.QueueId, queueId)
            .OrderBy(c => c.Position) // Упорядочиваем по позиции
            .ToListAsync();

        // Находим последнего клиента, который начал обслуживание
        var lastStartedClient = clientsInQueue
            .Where(c => !string.IsNullOrEmpty(c.StartTime))
            .OrderByDescending(c => c.Position)
            .FirstOrDefault();

        if (lastStartedClient == null)
        {
            // Очередь пустая или никто не начал обслуживание
            return TimeSpan.Zero;
        }

        // Время начала обслуживания последнего клиента
        var lastClientStartTime = DateTime.Parse(lastStartedClient.StartTime);

        // Время, прошедшее с начала обслуживания последнего клиента
        var timeElapsed = DateTime.Now - lastClientStartTime;

        var lastClientServiceEntity = await _serviceRepository.GetByIdAsync(queueServiceEntity.ServiceId);
        var lastClientServiceAverageTime = TimeSpan.Parse(lastClientServiceEntity.AverageTime);

        // Оставшееся время на обслуживание последнего клиента
        var remainingTimeForLastClient = lastClientServiceAverageTime - timeElapsed;

        var totalWaitTime = remainingTimeForLastClient > TimeSpan.Zero
            ? remainingTimeForLastClient
            : TimeSpan.Zero;

        foreach (var clientInQueue in clientsInQueue.Where(c => c.Position > client.Position))
        {
            var clientServiceEntity = await _serviceRepository.GetByIdAsync(clientInQueue.QueueServiceId);
            var clientServiceAverageTime = TimeSpan.Parse(clientServiceEntity.AverageTime);

            totalWaitTime += clientServiceAverageTime;
        }

        return totalWaitTime;
    }
}