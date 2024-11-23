namespace Domain.Entities;

using Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Queues")]
public class QueueEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [ForeignKey("Organization")] public long OrganizationId { get; set; }

    [Required] public int WindowNumber { get; set; }

    public QueueEntity FromDomain(DynamicQueue domainEntity)
    {
        return new QueueEntity
        {
            OrganizationId = domainEntity.Organization.Id,
            WindowNumber = domainEntity.WindowNumber
        };
    }
}