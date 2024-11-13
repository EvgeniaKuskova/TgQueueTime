using Infrastructure.Entities;

namespace Domain;

public class Client:DomainService<Client, QueueClientsEntity>
{
    public long Id { get; }
    public Service Service;

    public Client(long id, Service service)
    {
        Id = id;
        Service = service;
    }
}