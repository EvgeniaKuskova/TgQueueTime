using System.Linq.Expressions;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task DeleteAsync(long id);
    Task UpdateAsync(T entity);
    Task<T> GetByIdAsync(long id);
    IQueryable<T> GetAllByValueAsync<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value);
    Task<T> GetByConditionsAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> GetAllByCondition(Expression<Func<T, bool>> predicate);

}