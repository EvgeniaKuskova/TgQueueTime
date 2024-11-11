namespace Domain;

public class QueueItem
{
    public Client Client { get; set; }
    public DateTime ServiceStartTime { get; set; }

    public QueueItem(Client client, DateTime serviceStartTime)
    {
        Client = client;
        ServiceStartTime = serviceStartTime;
    }

    public TimeSpan TimeUntilServiceStart => ServiceStartTime - DateTime.Now;
}

public class DynamicQueue
{
    public readonly List<QueueItem> Queue = new();
    public readonly List<Service> Services;
    public Organization Organization;
    public int WindowNumber;
    public DateTime StartTime { get;}

    public DynamicQueue(List<Service> services, Organization organization, int windowNumber)
    {
        Services = services;
        Organization = organization;
        StartTime = DateTime.Now;
        WindowNumber = windowNumber;
    }

    public void AddClient(Client client)
    {
        var time = TimeSpan.Zero;
        if (Queue.Any())
            time = Queue.Last().TimeUntilServiceStart + client.Service.AverageTime;
        var queueTime = DateTime.Now.Add(time);
        Queue.Add(new QueueItem(client, queueTime));
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
        if (Queue[0].ServiceStartTime < StartTime)
            Queue[0].ServiceStartTime = StartTime;
        else
            Queue[0].ServiceStartTime = currentTime > StartTime ? currentTime : StartTime;

        for (var i = 1; i < Queue.Count; i++)
            Queue[i].ServiceStartTime = Queue[i - 1].ServiceStartTime + client.Service.AverageTime;
    }
}