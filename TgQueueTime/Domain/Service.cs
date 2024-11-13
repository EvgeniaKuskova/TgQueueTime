using Infrastructure.Entities;

namespace Domain;

public class Service:DomainService<Service, ServiceEntity>
{
    public string Name;
    public TimeSpan AverageTime;

    public Service(string name, TimeSpan time)
    {
        Name = name;
        AverageTime = time;
    }
}