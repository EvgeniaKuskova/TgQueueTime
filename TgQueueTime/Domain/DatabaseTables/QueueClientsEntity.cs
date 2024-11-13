using Infrastructure;

namespace Domain.Entities;

using Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("QueueClients")]
public class QueueClientsEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [ForeignKey("Queues")] public long QueueId { get; set; }
    [Key] public long UserId { get; set; }
    [Required] public int Position { get; set; } 
    public int? StartTime { get; set; }

    public QueueClientsEntity FromDomain(Client domainEntity, long queueId, int position)
    {
        return new QueueClientsEntity
        {
            UserId = domainEntity.Id,
            QueueId = queueId,
            Position = position,
            StartTime = (int)domainEntity.Service.AverageTime.TotalMinutes
        };
    }

    public Client ToDomain(QueueClientsEntity databaseEntity, ApplicationDbContext context)
    {
        var serviceEntity = context.Services.Find(databaseEntity.QueueId); // Предполагается, что QueueId связан с Service
        var service = new ServiceEntity().ToDomain(serviceEntity, context);

        return new Client(databaseEntity.UserId, service);
    }

}