using Domain.Entities;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using TgQueueTime.Application;
using Xunit;

namespace Domain.Services;

public class QueueServiceTest
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<QueueEntity> _queueRepository;
    private readonly IRepository<QueueServicesEntity> _queueServicesRepository;
    private readonly IRepository<ClientsEntity> _clientRepository;
    private readonly IRepository<OrganizationEntity> _organizationRepository;
    private readonly IRepository<ServiceEntity?> _serviceRepository;
    private readonly QueueService _queueService;

    public QueueServiceTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Каждый тест работает с чистой БД
            .Options;

        _context = new ApplicationDbContext(options);

        _queueRepository = new Repository<QueueEntity>(_context);
        _queueServicesRepository = new Repository<QueueServicesEntity>(_context);
        _clientRepository = new Repository<ClientsEntity>(_context);
        _organizationRepository = new Repository<OrganizationEntity>(_context);
        _serviceRepository = new Repository<ServiceEntity?>(_context);

        _queueService = new QueueService(
            _queueRepository,
            _queueServicesRepository,
            _clientRepository,
            _organizationRepository,
            _serviceRepository);
    }

    [Fact]
    public async Task AddDataToDatabase_Should_SaveDataCorrectly()
    {
        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };

        var service = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:30:00"
        };

        var queue = new QueueEntity
        {
            WindowNumber = 1
        };

        await _context.Organizations.AddAsync(organization);
        await _context.SaveChangesAsync();

        service.OrganizationId = organization.Id;
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        queue.OrganizationId = organization.Id;
        await _context.Queues.AddAsync(queue);
        await _context.SaveChangesAsync();

        var queueService = new QueueServicesEntity
        {
            QueueId = queue.Id,
            ServiceId = service.Id
        };

        await _context.QueueServices.AddAsync(queueService);
        await _context.SaveChangesAsync();

        var client = new ClientsEntity
        {
            QueueId = queue.Id,
            UserId = 123,
            Position = 1,
            StartTime = DateTime.Now.ToString("o"),
            QueueServiceId = queueService.Id
        };

        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        var organizationInDb = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Name == organization.Name && o.Id == organization.Id);
        var serviceInDb = await _context.Services
            .FirstOrDefaultAsync(s => s.Name == service.Name && s.OrganizationId == organization.Id);
        var queueInDb = await _context.Queues
            .FirstOrDefaultAsync(q => q.WindowNumber == queue.WindowNumber && q.OrganizationId == organization.Id);
    }

    [Fact]
    public async Task AddClientToQueueAsync_Should_Add_Client_To_Optimal_Queue()
    {
        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };

        await _context.Organizations.AddAsync(organization);
        await _context.SaveChangesAsync();

        var service1 = new ServiceEntity
        {
            Name = "Test Service 1",
            AverageTime = "00:30:00",
            OrganizationId = organization.Id
        };

        var service2 = new ServiceEntity
        {
            Name = "Test Service 2",
            AverageTime = "00:20:00",
            OrganizationId = organization.Id
        };

        await _context.Services.AddRangeAsync(service1, service2);
        await _context.SaveChangesAsync();

        var queue1 = new QueueEntity
        {
            OrganizationId = organization.Id,
            WindowNumber = 1
        };

        var queue2 = new QueueEntity
        {
            OrganizationId = organization.Id,
            WindowNumber = 2
        };

        await _context.Queues.AddRangeAsync(queue1, queue2);
        await _context.SaveChangesAsync();

        var queueService1 = new QueueServicesEntity
        {
            QueueId = queue1.Id,
            ServiceId = service1.Id
        };

        var queueService2 = new QueueServicesEntity
        {
            QueueId = queue2.Id,
            ServiceId = service2.Id
        };

        await _context.QueueServices.AddRangeAsync(queueService1, queueService2);
        await _context.SaveChangesAsync();

        var client = new Client(123, new Service("Test Service 1", TimeSpan.FromMinutes(30)));

        var servicesProvided = await _context.Services
            .Where(s => s.OrganizationId == organization.Id)
            .Select(s => new Service(s.Name, TimeSpan.Parse(s.AverageTime)))
            .ToListAsync();

        var queueService = new QueueService(
            new Repository<QueueEntity>(_context),
            new Repository<QueueServicesEntity>(_context),
            new Repository<ClientsEntity>(_context),
            new Repository<OrganizationEntity>(_context),
            new Repository<ServiceEntity?>(_context));

        await queueService.AddClientToQueueAsync(client,
            new Organization(organization.Id, "Test Organization", servicesProvided));

        var clientsInQueue1 = await _context.Clients.Where(c => c.QueueId == queue1.Id).ToListAsync();
        var clientsInQueue2 = await _context.Clients.Where(c => c.QueueId == queue2.Id).ToListAsync();

        Assert.NotEmpty(clientsInQueue1); // Клиент добавлен в первую очередь
        Assert.Empty(clientsInQueue2); // Вторая очередь пуста
    }


    [Fact]
    public async Task MoveQueue_Should_Update_Queue_Correctly()
    {
        var organizationEntity = new OrganizationEntity { Name = "Test Organization" };
        await _organizationRepository.AddAsync(organizationEntity);
        await _context.SaveChangesAsync();

        var serviceEntity = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:05:00",
            OrganizationId = organizationEntity.Id
        };
        await _serviceRepository.AddAsync(serviceEntity);
        await _context.SaveChangesAsync();

        var queueEntity = new QueueEntity
        {
            OrganizationId = organizationEntity.Id,
            WindowNumber = 1
        };
        await _queueRepository.AddAsync(queueEntity);
        await _context.SaveChangesAsync();

        var queueServiceEntity = new QueueServicesEntity
        {
            QueueId = queueEntity.Id,
            ServiceId = serviceEntity.Id
        };
        await _queueServicesRepository.AddAsync(queueServiceEntity);
        await _context.SaveChangesAsync();

        var startTime = DateTime.Now.AddMinutes(-6).ToString("o"); // Клиент обслуживается уже 6 минут
        var clients = new List<ClientsEntity>
        {
            new ClientsEntity
            {
                QueueId = queueEntity.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 1,
                Position = 1,
                StartTime = startTime
            },
            new ClientsEntity
            {
                QueueId = queueEntity.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 2,
                Position = 2,
                StartTime = null
            },
            new ClientsEntity
            {
                QueueId = queueEntity.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 3,
                Position = 3,
                StartTime = null
            }
        };

        foreach (var client in clients)
        {
            await _clientRepository.AddAsync(client);
        }

        await _context.SaveChangesAsync();

        var organization = new Organization(organizationEntity.Id, organizationEntity.Name);

        await _queueService.MoveQueue(organization, 1);

        var remainingClients = await _clientRepository
            .GetAllByValueAsync(c => c.QueueId, queueEntity.Id)
            .OrderBy(c => c.Position)
            .ToListAsync();

        Assert.Equal(2, remainingClients.Count);
        Assert.DoesNotContain(remainingClients, c => c.UserId == 1);

        var nextClient = remainingClients.FirstOrDefault(c => c.UserId == 2);
        Assert.NotNull(nextClient);
        Assert.NotNull(nextClient.StartTime); // Следующий клиент должен получить StartTime
        Assert.Equal(2, nextClient.Position); // Позиция следующего клиента остается неизменной
    }

    [Fact]
    public async Task GetNumberClientsBeforeQuery_Should_Return_Correct_Count()
    {
        var queueId = 1L;

        var clients = new List<ClientsEntity>
        {
            new ClientsEntity { QueueId = queueId, UserId = 1, Position = 1, StartTime = null },
            new ClientsEntity { QueueId = queueId, UserId = 2, Position = 2, StartTime = null },
            new ClientsEntity { QueueId = queueId, UserId = 3, Position = 3, StartTime = DateTime.Now.ToString("o") },
            new ClientsEntity { QueueId = queueId, UserId = 4, Position = 4, StartTime = null }
        };

        foreach (var client in clients)
        {
            await _clientRepository.AddAsync(client);
        }

        await _context.SaveChangesAsync();

        var testClient = clients[3];

        var queueService = new QueueService(null, null, _clientRepository, null, null);

        var countBefore = await queueService.GetNumberClientsBeforeQuery(testClient);

        Assert.Equal(2, countBefore);
    }

    [Fact]
    public async Task GetAllClientsInQueueQuery_Should_Return_All_Clients_In_Queue()
    {
        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await _organizationRepository.AddAsync(organization);
        await _context.SaveChangesAsync();

        var service = new ServiceEntity
        {
            Name = "Test Service",
            AverageTime = "00:05:00",
            OrganizationId = organization.Id
        };
        await _serviceRepository.AddAsync(service);
        await _context.SaveChangesAsync();

        var queue = new QueueEntity
        {
            OrganizationId = organization.Id,
            WindowNumber = 1
        };
        await _queueRepository.AddAsync(queue);
        await _context.SaveChangesAsync();

        var queueServiceEntity = new QueueServicesEntity
        {
            QueueId = queue.Id,
            ServiceId = service.Id
        };
        await _queueServicesRepository.AddAsync(queueServiceEntity);
        await _context.SaveChangesAsync();

        var clients = new List<ClientsEntity>
        {
            new ClientsEntity
            {
                QueueId = queue.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 1,
                Position = 1,
                StartTime = DateTime.Now.ToString("o")
            },
            new ClientsEntity
            {
                QueueId = queue.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 2,
                Position = 2,
                StartTime = null
            },
            new ClientsEntity
            {
                QueueId = queue.Id,
                QueueServiceId = queueServiceEntity.Id,
                UserId = 3,
                Position = 3,
                StartTime = null
            }
        };

        foreach (var client in clients)
        {
            await _clientRepository.AddAsync(client);
        }

        await _context.SaveChangesAsync();

        var result = await _queueService.GetAllClientsInQueueQuery(
            new Organization(organization.Id, organization.Name), 1);

        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(1, result.Value[0].Id); // Проверка, что клиенты идут в правильном порядке
        Assert.Equal(2, result.Value[1].Id);
        Assert.Equal(3, result.Value[2].Id);
    }


    [Fact]
    public async Task GetAllServices_Should_Return_All_Services_For_Organization()
    {
        var organization = new OrganizationEntity
        {
            Name = "Test Organization"
        };
        await _organizationRepository.AddAsync(organization);
        await _context.SaveChangesAsync();

        var service1 = new ServiceEntity
        {
            Name = "Test Service 1",
            AverageTime = "00:15:00",
            OrganizationId = organization.Id
        };
        var service2 = new ServiceEntity
        {
            Name = "Test Service 2",
            AverageTime = "00:30:00",
            OrganizationId = organization.Id
        };
        await _serviceRepository.AddAsync(service1);
        await _serviceRepository.AddAsync(service2);
        await _context.SaveChangesAsync();

        var domainOrganization = organization.ToDomain(_serviceRepository);

        var services = await _queueService.GetAllServices(domainOrganization);

        Assert.NotNull(services);
        Assert.Equal(2, services.Count);

        var serviceNames = services.Select(s => s.Name).ToList();
        Assert.Contains("Test Service 1", serviceNames);
        Assert.Contains("Test Service 2", serviceNames);

        var serviceAverageTimes = services.Select(s => s.AverageTime).ToList();
        Assert.Contains(TimeSpan.FromMinutes(15), serviceAverageTimes);
        Assert.Contains(TimeSpan.FromMinutes(30), serviceAverageTimes);
    }

    [Fact]
    public async Task CreateQueueAsync_Should_Throw_NotImplementedException()
    {
        var organization = new Organization(1, "Test Organization");

        await Assert.ThrowsAsync<NotImplementedException>(async () =>
            await _queueService.CreateQueueAsync(organization, 1));
    }

    [Fact]
    public async Task GetClientTimeQuery_Should_Return_Correct_Wait_Time()
    {
        var service = new ServiceEntity { Name = "Test Service", AverageTime = "00:10:00" };
        await _serviceRepository.AddAsync(service);
        await _context.SaveChangesAsync();

        var queueServiceEntity = new QueueServicesEntity
        {
            ServiceId = service.Id,
            QueueId = 1
        };
        await _queueServicesRepository.AddAsync(queueServiceEntity);
        await _context.SaveChangesAsync();

        var clientEntity = new ClientsEntity
        {
            QueueId = 1,
            Position = 1,
            StartTime = null,
            QueueServiceId = queueServiceEntity.Id
        };
        await _clientRepository.AddAsync(clientEntity);
        await _context.SaveChangesAsync();

        var waitTime = await _queueService.GetClientTimeQuery(clientEntity);

        Assert.Equal(TimeSpan.FromMinutes(0), waitTime); // очередь не запущена
    }
}