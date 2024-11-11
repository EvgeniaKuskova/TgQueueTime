namespace Domain;

public class Organization
{
    public readonly long Id;
    public readonly string Name;
    public List<Service> Services;
    public int WindowCount;

    public Organization(long id, string name, List<Service> services, int windowCount)
    {
        Id = id;
        Name = name;
        Services = services;
        WindowCount = windowCount;
    }
}