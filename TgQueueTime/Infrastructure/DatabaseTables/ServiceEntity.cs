namespace Infrastructure.Entities;
using Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Services")]
public class ServiceEntity : EntityMapperBase<Service, ServiceEntity>
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

    public override ServiceEntity FromDomain(Service domainEntity)
    {
        return new ServiceEntity
        {
            Name = domainEntity.Name,
            AverageTime = domainEntity.AverageTime.ToString()
        };
    }

    public override Service ToDomain(ServiceEntity databaseEntity, ApplicationDbContext context)
    {
        return new Service(
            databaseEntity.Name,
            TimeSpan.Parse(databaseEntity.AverageTime));
    }
}