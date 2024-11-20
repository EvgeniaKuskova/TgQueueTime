using Infrastructure;
using Infrastructure.Repositories;

namespace Domain.Entities;
using Domain;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // перенести в Domain

[Table("Organizations")]
public class OrganizationEntity
{
    public long Id { get; set; }
    [Required] [MaxLength(100)] public string Name { get; set; }

    public OrganizationEntity()
    {
    }

    public OrganizationEntity FromDomain(Organization domainEntity)
    {
        return new OrganizationEntity
        {
            Name = domainEntity.Name
        };
    }

    public Organization ToDomain(IRepository<ServiceEntity> serviceRepository)
    {
        // Получаем все услуги, связанные с этой организацией
        var services = serviceRepository
            .GetAllByValueAsync(s => s.OrganizationId, this.Id)
            .ToList(); // Синхронный вызов для простоты, но лучше использовать асинхронные методы.

        // Преобразуем услуги в доменные объекты
        var domainServices = services
            .Select(service => new Service(service.Name, TimeSpan.Parse(service.AverageTime)))
            .ToList();

        // Создаем и возвращаем объект доменной модели Organization
        return new Organization(this.Id, this.Name, domainServices);
    }


}