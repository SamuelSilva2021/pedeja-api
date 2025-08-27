using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Infrastructure.Data;

namespace PedejaApp.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // Métodos básicos existentes
    public virtual async Task<IEnumerable<T>> GetAllAsync() => 
        await _dbSet.ToListAsync();

    public virtual async Task<T?> GetByIdAsync(int id) => 
        await _dbSet.FirstOrDefaultAsync(x => x.Id == id);

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteVirtualAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(x => x.Id == id);
    }

    public async Task DeleteAsync(T Entity)
    {
        _dbSet.Remove(Entity);
        await _context.SaveChangesAsync();
    }

    // Métodos com predicates e consultas complexas
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null 
            ? await _dbSet.CountAsync() 
            : await _dbSet.CountAsync(predicate);
    }

    // Métodos com paginação
    public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
    {
        return await _dbSet
            .Where(predicate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // Métodos com ordenação
    public virtual async Task<IEnumerable<T>> GetAllOrderedAsync<TKey>(Expression<Func<T, TKey>> orderBy, bool ascending = true)
    {
        return ascending 
            ? await _dbSet.OrderBy(orderBy).ToListAsync()
            : await _dbSet.OrderByDescending(orderBy).ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindOrderedAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBy, bool ascending = true)
    {
        var query = _dbSet.Where(predicate);
        return ascending 
            ? await query.OrderBy(orderBy).ToListAsync()
            : await query.OrderByDescending(orderBy).ToListAsync();
    }

    // Métodos com includes para relacionamentos
    public virtual async Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes)
    {
        var query = includes.Aggregate(_dbSet.AsQueryable(), (current, include) => current.Include(include));
        return await query.ToListAsync();
    }

    public virtual async Task<T?> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes)
    {
        var query = includes.Aggregate(_dbSet.AsQueryable(), (current, include) => current.Include(include));
        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public virtual async Task<IEnumerable<T>> FindWithIncludesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        var query = includes.Aggregate(_dbSet.AsQueryable(), (current, include) => current.Include(include));
        return await query.Where(predicate).ToListAsync();
    }

    // Operações em lote
    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        var entitiesList = entities.ToList();
        var now = DateTime.UtcNow;
        
        foreach (var entity in entitiesList)
        {
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
        }
        
        await _dbSet.AddRangeAsync(entitiesList);
        await _context.SaveChangesAsync();
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        var entitiesList = entities.ToList();
        var now = DateTime.UtcNow;
        
        foreach (var entity in entitiesList)
        {
            entity.UpdatedAt = now;
        }
        
        _dbSet.UpdateRange(entitiesList);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

    // Métodos de agregação
    public virtual async Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector)
    {
        return await _dbSet.MaxAsync(selector);
    }

    public virtual async Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector)
    {
        return await _dbSet.MinAsync(selector);
    }

    public virtual async Task<decimal> SumAsync(Expression<Func<T, decimal>> selector)
    {
        return await _dbSet.SumAsync(selector);
    }

    public virtual async Task<double> AverageAsync(Expression<Func<T, decimal>> selector)
    {
        return (double)await _dbSet.AverageAsync(selector);
    }
}
