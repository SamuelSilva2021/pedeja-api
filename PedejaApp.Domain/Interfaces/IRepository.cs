using System.Linq.Expressions;
using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    // Métodos básicos existentes
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteVirtualAsync(int id);
    Task DeleteAsync(T Entity);
    Task<bool> ExistsAsync(int id);

    // Métodos com predicates e consultas complexas
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    
    // Métodos com paginação
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize);
    
    // Métodos com ordenação
    Task<IEnumerable<T>> GetAllOrderedAsync<TKey>(Expression<Func<T, TKey>> orderBy, bool ascending = true);
    Task<IEnumerable<T>> FindOrderedAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBy, bool ascending = true);
    
    // Métodos com includes para relacionamentos
    Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes);
    Task<T?> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> FindWithIncludesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    
    // Operações em lote
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    
    // Métodos de agregação
    Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector);
    Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector);
    Task<decimal> SumAsync(Expression<Func<T, decimal>> selector);
    Task<double> AverageAsync(Expression<Func<T, decimal>> selector);
}

