using Infrastructure;
using Infrastructure.Repositories;

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

    public Client ToDomain(IRepository<QueueServicesEntity> queueServicesRepository, IRepository<ServiceEntity> serviceRepository)
    {
        // Получаем связь QueueServiceEntity
        var queueServiceEntity = queueServicesRepository.GetByIdAsync(this.QueueServiceId).Result;
        if (queueServiceEntity == null)
        {
            throw new InvalidOperationException($"QueueService with ID {this.QueueServiceId} not found.");
        }

        // Получаем связанный ServiceEntity
        var serviceEntity = serviceRepository.GetByIdAsync(queueServiceEntity.ServiceId).Result;
        if (serviceEntity == null)
        {
            throw new InvalidOperationException($"Service with ID {queueServiceEntity.ServiceId} not found.");
        }

        // Преобразуем ServiceEntity в доменный объект Service
        var service = new Service(serviceEntity.Name, TimeSpan.Parse(serviceEntity.AverageTime));

        // Создаем и возвращаем доменный объект Client
        return new Client(this.UserId, service);
    }


}