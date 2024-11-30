using System;
using System.Linq;

namespace Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

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
            Name = domainEntity.Name,
            Id = domainEntity.Id
        };
    }

    public Organization ToDomain(IRepository<ServiceEntity?> serviceRepository)
    {
        var services = serviceRepository
            .GetAllByValueAsync(s => s.OrganizationId, this.Id)
            .ToList();

        var domainServices = services
            .Select(service => new Service(service.Name, TimeSpan.Parse(service.AverageTime)))
            .ToList();

        return new Organization(this.Id, this.Name, domainServices);
    }
    

}