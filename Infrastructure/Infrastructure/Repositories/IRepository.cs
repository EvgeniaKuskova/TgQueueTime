using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task DeleteAsync(long id);
    Task UpdateAsync(T entity);
    Task<T?> GetByKeyAsync(params object[] key);
    IQueryable<T> GetAllByValueAsync<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value);
    Task<T> GetByConditionsAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> GetAllByCondition(Expression<Func<T, bool>> predicate);

}