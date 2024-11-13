using Domain.Entities;

namespace Domain;

public class Client
{
    public long Id { get; }
    public Service Service;

    public Client(long id, Service service)
    {
        Id = id;
        Service = service;
    }
}