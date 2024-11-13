using Domain;
using Infrastructure;
using Infrastructure.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace TgQueueTime.Application;

public class Commands
{
    public async Task RegisterOrganizationCommand(long idOrganization, string organizationName, int windowCount)
    {
        var organization = new Organization(idOrganization, organizationName, windowCount);
        //Service.RegisterOrg(org);
        //RepositoryOrg.RegisterOrg(org);
        //репозиторий - (сервис/хранилка) для каждой сущности
        
        
        
        /*var serviceProvider = Startup.ConfigureServices();
        var domainService = serviceProvider.GetService<IDomain<Organization,OrganizationEntity>>();
        await domainService.PutInDataBaseAsync(organization);*/
    }

    void AddClientToQueueCommand(long idClient, string serviceName, string organizationName)
    {
        // заглушка
    }

    void UpdateServiceAverageTimeCommand(long idOrganization, string serviceName, TimeSpan averageTime)
    {
        // заглушка
    }

    void AddService(long idOrganization, string serviceName, TimeSpan averageTime, List<int> windowNumbers)
    {
        // заглушка
    }
}