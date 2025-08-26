using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetActiveProductsByCategoryAsync(int categoryId);
    Task<int> GetNextDisplayOrderAsync(int categoryId);
    Task UpdateDisplayOrdersAsync(Dictionary<int, int> productDisplayOrders);
    Task<bool> IsNameUniqueInCategoryAsync(string name, int categoryId, int? excludeId = null);
    Task<bool> HasProductsInCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
    Task ReorderProductsAsync(Dictionary<int, int> productOrders);
    Task<IEnumerable<Product>> GetProductsForMenuAsync();
}
