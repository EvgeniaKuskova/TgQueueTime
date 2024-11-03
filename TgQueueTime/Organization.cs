namespace Domain;

public class Organization
{
    public readonly long Id;
    public readonly string Name;
    public List<string> Services;

    public Organization(long id, string name, List<string> services)
    {
        Id = id;
        Name = name;
        Services = services;
    }
}