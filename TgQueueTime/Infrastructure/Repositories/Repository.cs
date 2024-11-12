using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task DeleteAsync(long id);
    Task UpdateAsync(T entity);
    Task<T> GetByIdAsync(long id);
}


public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    
    public Repository(ApplicationDbContext context)
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

    public async Task<T> GetByIdAsync(long id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<List<T>> GetAllByValueAsync<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value)
    {
        var memberExpression = propertySelector.Body as MemberExpression;
        if (memberExpression == null)
            throw new ArgumentException("Выражение должно быть свойством.", nameof(propertySelector));

        var propertyName = memberExpression.Member.Name;
        return await _context.Set<T>()
            .Where(e => EF.Property<TProperty>(e, propertyName).Equals(value))
            .ToListAsync();
    }
    
}
