namespace Infrastructure.Entities;

using Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("QueueServices")]
public class QueueServicesEntity : EntityMapperBase<DynamicQueue, QueueServicesEntity>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [ForeignKey("Queues")] public long QueueId { get; set; }
    [ForeignKey("Services")] public long ServiceId { get; set; }

    public override QueueServicesEntity FromDomain(DynamicQueue domainEntity)
    {
        // заглушка
        return new QueueServicesEntity();
    }

    public override DynamicQueue ToDomain(QueueServicesEntity databaseEntity, ApplicationDbContext context)
    {
        // заглушка
        return new DynamicQueue(new List<Service>(), new Organization(1L, "aboba", 1), 1) { };
    }
}