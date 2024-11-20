
using Infrastructure;
using Infrastructure.Repositories;

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

    public DynamicQueue ToDomain(IRepository<OrganizationEntity> organizationRepository, IRepository<QueueServicesEntity> queueServicesRepository, IRepository<ServiceEntity> serviceRepository)
    {
        // Получаем организацию
        var organizationEntity = organizationRepository.GetByIdAsync(this.OrganizationId).Result; // Для упрощения, но лучше использовать async/await
        if (organizationEntity == null)
        {
            throw new InvalidOperationException($"Organization with ID {this.OrganizationId} not found.");
        }

        var organization = organizationEntity.ToDomain(serviceRepository);

        // Получаем услуги, связанные с этой очередью
        var queueServices = queueServicesRepository
            .GetAllByValueAsync(qs => qs.QueueId, this.Id)
            .ToList(); 

        var serviceIds = queueServices.Select(qs => qs.ServiceId).ToList();
        var serviceEntities = serviceRepository
            .GetAllByCondition(s => serviceIds.Contains(s.Id))
            .ToList(); 

        var services = serviceEntities
            .Select(se => new ServiceEntity().ToDomain())
            .ToList();

        return new DynamicQueue(services, organization, this.WindowNumber);
    }


}