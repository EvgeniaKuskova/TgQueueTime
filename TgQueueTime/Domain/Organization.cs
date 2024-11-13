using Domain.Entities;

namespace Domain;

public class Organization
{
    public readonly long Id;
    public readonly string Name;
    public List<Service> Services = new();
    public int WindowCount;

    public Organization(long id, string name, List<Service> services, int windowCount)
    {
        Id = id;
        Name = name;
        WindowCount = windowCount;
    }
    
    public Organization(long id, string name, int windowCount)
    {
        Id = id;
        Name = name;
        WindowCount = windowCount;
    }

    public void AddService(Service service)
    {
        Services.Add(service);
    }
}