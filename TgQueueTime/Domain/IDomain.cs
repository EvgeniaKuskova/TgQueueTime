using Infrastructure.Repositories;
using Infrastructure;
using Infrastructure.Entities;
using Microsoft.Extensions.DependencyInjection;
namespace Domain;

public interface IDomain<TDomainEntity, TDatabaseEntity> 
    where TDatabaseEntity : class
{
    Task PutInDataBaseAsync(TDomainEntity domainEntity);
}

public class DomainService<TDomainEntity, TDatabaseEntity> : IDomain<TDomainEntity, TDatabaseEntity>
    where TDatabaseEntity : class
{
    private readonly IRepository<TDatabaseEntity> _repository;
    private readonly IEntityMapper<TDomainEntity, TDatabaseEntity> _mapper;

    public DomainService(IRepository<TDatabaseEntity> repository, IEntityMapper<TDomainEntity, TDatabaseEntity> mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    protected DomainService()
    {
    }

    public async Task PutInDataBaseAsync(TDomainEntity domainEntity)
    {
        var databaseEntity = _mapper.FromDomain(domainEntity);
        await _repository.AddAsync(databaseEntity);
    }
}
