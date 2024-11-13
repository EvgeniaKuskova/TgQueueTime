
using Infrastructure;

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

    public DynamicQueue ToDomain(QueueEntity databaseEntity, ApplicationDbContext context)
    {
        var organizationEntity = context.Organizations.Find(databaseEntity.OrganizationId);
        var organization = new OrganizationEntity().ToDomain(organizationEntity, context);

        // Получаем связанные услуги
        var serviceEntities = context.QueueServices
            .Where(qs => qs.QueueId == databaseEntity.Id)
            .Select(qs => context.Services.Find(qs.ServiceId))
            .ToList();

        var services = serviceEntities.Select(se => new ServiceEntity().ToDomain(se, context)).ToList();

        return new DynamicQueue(services, organization, databaseEntity.WindowNumber);
    }

}