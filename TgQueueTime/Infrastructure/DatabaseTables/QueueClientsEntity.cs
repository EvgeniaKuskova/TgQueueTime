namespace Infrastructure.Entities;

using Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("QueueClients")]
public class QueueClientsEntity : EntityMapperBase<Client, QueueClientsEntity>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [ForeignKey("Queues")] public long QueueId { get; set; }
    [Key] public long UserId { get; set; }
    [Required] public int Position { get; set; } 
    public int? StartTime { get; set; }

    public override QueueClientsEntity FromDomain(Client domainEntity)
    {
        // заглушка
        return new QueueClientsEntity();
    }

    public override Client ToDomain(QueueClientsEntity databaseEntity, ApplicationDbContext context)
    {
        // заглушка
        return new Client(databaseEntity.Id, new Service("aboba", TimeSpan.Zero)) { };
    }
}