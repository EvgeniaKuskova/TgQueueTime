using Infrastructure;

namespace Domain.Entities;
using Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Services")]
public class ServiceEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [ForeignKey("Organization")]
    public long OrganizationId { get; set; }

    [Required] [MaxLength(100)] public string Name { get; set; }

    [Required] public string AverageTime { get; set; } // Храним TimeSpan как строку

    public ServiceEntity()
    {
    }

    public ServiceEntity FromDomain(Service domainEntity)
    {
        return new ServiceEntity
        {
            Name = domainEntity.Name,
            AverageTime = domainEntity.AverageTime.ToString()
        };
    }

    public Service ToDomain()
    {
        return new Service(
            this.Name,
            TimeSpan.Parse(this.AverageTime));
    }
}