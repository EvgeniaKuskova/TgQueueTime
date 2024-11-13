using Domain;

namespace Infrastructure.Entities;

public interface IEntityMapper<TDomainEntity, TDatabaseEntity> // убрать вообще, чтобы не говорить, что у каждой таблицы есть сущность domain
{
    TDatabaseEntity FromDomain(TDomainEntity domainEntity);
    TDomainEntity ToDomain(TDatabaseEntity databaseEntity, ApplicationDbContext context);
}

public abstract class EntityMapperBase<TDomainEntity, TDatabaseEntity> : IEntityMapper<TDomainEntity, TDatabaseEntity>
{
    public abstract TDatabaseEntity FromDomain(TDomainEntity domainEntity);
    public abstract TDomainEntity ToDomain(TDatabaseEntity databaseEntity, ApplicationDbContext context);
}
