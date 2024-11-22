namespace Domain.Entities;

using Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("QueueClients")]
public class ClientsEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [ForeignKey("Queues")] public long QueueId { get; set; }
    [Key] public long UserId { get; set; }
    [Required] public int Position { get; set; } 
    public string? StartTime { get; set; }
    
    public long QueueServiceId { get; set; }

    public ClientsEntity FromDomain(Client domainEntity, long queueId, int position)
    {
        return new ClientsEntity
        {
            UserId = domainEntity.Id,
            QueueId = queueId,
            Position = position,
            StartTime = domainEntity.Service.AverageTime.TotalMinutes.ToString()
        };
    }
    
}