namespace Domain;

public class QueueItem
{
    public Client Client { get; set; }
    public DateTime ServiceStartTime { get; set; }

    public QueueItem(Client client, DateTime serviceStartTime, TimeSpan serviceDuration)
    {
        Client = client;
        ServiceStartTime = serviceStartTime;
    }

    public TimeSpan TimeUntilServiceStart => ServiceStartTime - DateTime.Now;
}

public class DynamicQueue
{
    public readonly List<QueueItem> Queue = new();
    private readonly Service _service;
    private Organization _organization;
    private readonly TimeSpan _beginTime;
    private readonly TimeSpan _endTime;
    private DateTime StartTime { get;}

    public DynamicQueue(Service service, Organization organization, TimeSpan beginTime, TimeSpan endTime)
    {
        _service = service;
        _organization = organization;
        _beginTime = beginTime;
        _endTime = endTime;
        StartTime = DateTime.Now;
    }

    public void AddClient(Client client)
    {
        var time = _beginTime;
        if (Queue.Any())
            time = Queue.Last().TimeUntilServiceStart + _service.AverageTime;

        if (time > _endTime)
            return;

        var queueTime = DateTime.Now.Add(time);
        Queue.Add(new QueueItem(client, queueTime, _service.AverageTime));
        Console.WriteLine($"Клиент {client.Id} добавлен в очередь на {queueTime:HH:mm:ss}");
    }

    public void DeleteClient(Client client)
    {
        var item = Queue.FirstOrDefault(q => q.Client.Id == client.Id);
        if (item != null) Queue.Remove(item);

        if (!Queue.Any())
        {
            Console.WriteLine("Очередь пуста после удаления клиента.");
            return;
        }

        var currentTime = DateTime.Now;
        if (Queue[0].ServiceStartTime < StartTime.Add(_beginTime))
            Queue[0].ServiceStartTime = StartTime.Add(_beginTime);
        else
            Queue[0].ServiceStartTime = currentTime > StartTime.Add(_beginTime) ? currentTime : StartTime.Add(_beginTime);

        for (var i = 1; i < Queue.Count; i++)
            Queue[i].ServiceStartTime = Queue[i - 1].ServiceStartTime + _service.AverageTime;
    }
}