using Domain.Entities;

namespace Domain;

public class Service
{
    public string Name;
    public TimeSpan AverageTime;

    public Service(string name, TimeSpan time)
    {
        Name = name;
        AverageTime = time;
    }
}