using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services;

public class QueueService
{
    private readonly IRepository<QueueEntity> _queueRepository;
    private readonly IRepository<QueueServicesEntity> _queueServicesRepository;
    private readonly IRepository<ClientsEntity> _clientRepository;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<ServiceEntity?> _serviceRepository;

    public QueueService(
        IRepository<QueueEntity> queueRepository,
        IRepository<QueueServicesEntity> queueServicesRepository,
        IRepository<ClientsEntity> clientRepository,
        IRepository<OrganizationEntity> organizationRepository,
        IRepository<ServiceEntity?> serviceRepository)
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
                var lastService = await _serviceRepository.GetByKeyAsync(lastServiceId);

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
                var clientService = await _serviceRepository.GetByKeyAsync(clientServiceId);

                queueStartTime += TimeSpan.Parse(clientService.AverageTime);
            }

            // Проверяем, является ли эта очередь более оптимальной
            if (queueStartTime < optimalStartTime)
            {
                optimalStartTime = queueStartTime;
                optimalQueue = queue;
            }
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

    public Task CreateQueueAsync(Organization organization, int windowNumber)
    {
        throw new NotImplementedException(); //!!!!!!!!!!!!!!!!
    }

    public async Task<TimeSpan> GetClientTimeQuery(ClientsEntity client)
    {
        var queueServiceEntity = await _queueServicesRepository.GetByKeyAsync(client.QueueServiceId);
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

        var lastClientServiceEntity = await _serviceRepository.GetByKeyAsync(queueServiceEntity.ServiceId);
        var lastClientServiceAverageTime = TimeSpan.Parse(lastClientServiceEntity.AverageTime);

        // Оставшееся время на обслуживание последнего клиента
        var remainingTimeForLastClient = lastClientServiceAverageTime - timeElapsed;

        var totalWaitTime = remainingTimeForLastClient > TimeSpan.Zero
            ? remainingTimeForLastClient
            : TimeSpan.Zero;

        foreach (var clientInQueue in clientsInQueue.Where(c => c.Position > client.Position))
        {
            var clientServiceEntity = await _serviceRepository.GetByKeyAsync(clientInQueue.QueueServiceId);
            var clientServiceAverageTime = TimeSpan.Parse(clientServiceEntity.AverageTime);

            totalWaitTime += clientServiceAverageTime;
        }

        return totalWaitTime;
    }

    public async Task MoveQueue(Organization organization, int windowNumber)
    {
        var queueEntity = await _queueRepository.GetByConditionsAsync(
            q => q.OrganizationId == organization.Id && q.WindowNumber == windowNumber);

        if (queueEntity == null)
        {
            throw new InvalidOperationException(
                $"Очередь для окна {windowNumber} в организации с ID {organization.Id} не найдена.");
        }

        var clientsInQueue = await _clientRepository
            .GetAllByValueAsync(c => c.QueueId, queueEntity.Id)
            .OrderBy(c => c.Position)
            .ToListAsync();

        // Находим первого клиента с не пустым StartTime, который обслуживается сейчас
        var currentClient = clientsInQueue.FirstOrDefault(c => !string.IsNullOrEmpty(c.StartTime));

        if (currentClient != null)
        {
            var queueServiceEntity = await _queueServicesRepository.GetByKeyAsync(currentClient.QueueServiceId);
            if (queueServiceEntity == null)
            {
                throw new InvalidOperationException(
                    $"Связь QueueService для клиента с ID {currentClient.Id} не найдена.");
            }

            var serviceEntity = await _serviceRepository.GetByKeyAsync(queueServiceEntity.ServiceId);
            if (serviceEntity == null)
            {
                throw new InvalidOperationException($"Услуга с ID {queueServiceEntity.ServiceId} не найдена.");
            }

            await _clientRepository.DeleteAsync(currentClient.Id);
        }

        // Находим следующего клиента, который ожидает обслуживания
        var nextClient = clientsInQueue.FirstOrDefault(c => string.IsNullOrEmpty(c.StartTime));

        if (nextClient == null)
        {
            throw new InvalidOperationException(
                $"В очереди для окна {windowNumber} нет клиентов, ожидающих обслуживания.");
        }

        nextClient.StartTime = DateTime.Now.ToString("o");
        await _clientRepository.UpdateAsync(nextClient);
    }


    public async Task<int> GetNumberClientsBeforeQuery(ClientsEntity clientEntity)
    {
        var clientsInQueue = await _clientRepository
            .GetAllByValueAsync(c => c.QueueId, clientEntity.QueueId)
            .Where(c => c.StartTime == null) // Считаем только тех, у кого StartTime == null
            .OrderBy(c => c.Position) 
            .ToListAsync();

        var numberOfClientsBefore = clientsInQueue.Count(c => c.Position < clientEntity.Position);

        return numberOfClientsBefore;
    }

    public async Task<List<Client>> GetAllClientsInQueueQuery(Organization organization, int windowNumber)
    {
        var queueEntity = await _queueRepository.GetByConditionsAsync(
            q => q.OrganizationId == organization.Id && q.WindowNumber == windowNumber);

        if (queueEntity == null)
        {
            throw new InvalidOperationException(
                $"Очередь для окна {windowNumber} в организации {organization.Name} не найдена.");
        }

        var clientsInQueue = await _clientRepository
            .GetAllByValueAsync(c => c.QueueId, queueEntity.Id)
            .OrderBy(c => c.Position)
            .ToListAsync();

        // Преобразуем сущности клиентов в доменные модели
        var domainClients = clientsInQueue.Select(clientEntity =>
        {
            var queueServiceId = clientEntity.QueueServiceId;
            var serviceId = _queueServicesRepository.GetByConditionsAsync(key => key.Id == queueServiceId).Result.ServiceId;
            var serviceEntity = _serviceRepository.GetByConditionsAsync(key => key.Id == serviceId).Result;
            var service = new Service(serviceEntity.Name, TimeSpan.Parse(serviceEntity.AverageTime));
            return new Client(clientEntity.UserId, service);
        }).ToList();

        return domainClients;
    }

    public async Task<List<Service>> GetAllServices(Organization organization)
    {
        var serviceEntities = await _serviceRepository
            .GetAllByValueAsync(s => s.OrganizationId, organization.Id)
            .ToListAsync();

        var services = serviceEntities
            .Select(serviceEntity => new Service(serviceEntity.Name, TimeSpan.Parse(serviceEntity.AverageTime)))
            .ToList();

        return services;
    }

}