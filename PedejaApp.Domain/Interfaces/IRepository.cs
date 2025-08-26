using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteVirtualAsync(int id);
    Task DeleteAsync(T Entity);
    Task<bool> ExistsAsync(int id);
}

