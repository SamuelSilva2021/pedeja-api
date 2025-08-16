using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces
{
    public interface IProductRepository
    {
        // CRUD Operations
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // Business Operations
        Task<IEnumerable<Product>> GetProductsForMenuAsync();
        Task<int> GetNextDisplayOrderAsync(int categoryId);
        Task ReorderProductsAsync(Dictionary<int, int> productOrders);
        Task<bool> HasProductsInCategoryAsync(int categoryId);

        // Search and Filter
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    }
}
