using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
    Task<IEnumerable<Category>> GetCategoriesOrderedAsync();
    Task<bool> HasProductsAsync(int categoryId);
    Task<int> GetNextDisplayOrderAsync();
    Task UpdateDisplayOrdersAsync(Dictionary<int, int> categoryDisplayOrders);
    Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
}
