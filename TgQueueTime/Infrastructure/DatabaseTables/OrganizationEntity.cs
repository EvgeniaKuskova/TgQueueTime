namespace Infrastructure.Entities;
using Domain;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // перенести в Domain

[Table("Organizations")]
public class OrganizationEntity : EntityMapperBase<Organization, OrganizationEntity>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required] [MaxLength(100)] public string Name { get; set; }

    [Required] public int WindowCount { get; set; }

    public OrganizationEntity()
    {
    }

    public override OrganizationEntity FromDomain(Organization domainEntity)
    {
        return new OrganizationEntity
        {
            Name = domainEntity.Name,
            WindowCount = domainEntity.WindowCount
        };
    }

    public override Organization ToDomain(OrganizationEntity databaseEntity, ApplicationDbContext context)
    {
        return new Organization(
            databaseEntity.Id,
            databaseEntity.Name,
            databaseEntity.WindowCount
            );
    }
}