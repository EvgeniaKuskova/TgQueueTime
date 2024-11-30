
using System.Collections.Generic;

namespace Domain;

public class Organization
{
    public readonly long Id;
    public readonly string Name;
    public List<Service> Services = new();

    public Organization(long id, string name, List<Service> services)
    {
        Id = id;
        Name = name;
    }
    
    public Organization(long id, string name)
    {
        Id = id;
        Name = name;
    }

    public void AddService(Service service)
    {
        Services.Add(service);
    }
}