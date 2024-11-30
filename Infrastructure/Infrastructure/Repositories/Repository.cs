using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;

    public Repository(DbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    // оно не работает
    public async Task<T?> GetByKeyAsync(params object[] key)
    {
        var a = _context.Set<T>();
        return await _context.Set<T>().FindAsync(key);
    }

    public IQueryable<T> GetAllByValueAsync<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value)
    {
        var memberExpression = propertySelector.Body as MemberExpression;
        if (memberExpression == null)
            throw new ArgumentException("Выражение должно быть свойством.", nameof(propertySelector));

        var propertyName = memberExpression.Member.Name;

        return _context.Set<T>().Where(e => EF.Property<TProperty>(e, propertyName).Equals(value));
    }

    public async Task<T> GetByConditionsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(predicate);
    }

    
    public IQueryable<T> GetAllByCondition(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().Where(predicate);
    }
    
}
