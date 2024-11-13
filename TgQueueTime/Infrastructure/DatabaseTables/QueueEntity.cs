
namespace Infrastructure.Entities;

using Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Queues")]
public class QueueEntity : EntityMapperBase<DynamicQueue, QueueEntity>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [ForeignKey("Organization")] public long OrganizationId { get; set; }

    [Required] public int WindowNumber { get; set; }

    public override QueueEntity FromDomain(DynamicQueue domainEntity)
    {
        return new QueueEntity
        {
            OrganizationId = domainEntity.Organization.Id,
            WindowNumber = domainEntity.Organization.WindowCount
        };
    }

    public override DynamicQueue ToDomain(QueueEntity databaseEntity, ApplicationDbContext context)
    {
        // заглушка
        return new DynamicQueue(new List<Service>(), new Organization(1L, "aboba", 1), 1) { };
    }
}