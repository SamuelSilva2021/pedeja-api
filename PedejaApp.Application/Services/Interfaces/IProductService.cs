using PedejaApp.Domain.Entities;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services.Interfaces
{
    public interface IProductService
    {
        // CRUD Operations
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);

        // Business Operations
        Task<IEnumerable<Product>> GetProductsForMenuAsync();
        Task ReorderProductsAsync(Dictionary<int, int> productOrders);
        Task<Product> ToggleProductStatusAsync(int id);
        Task<Product> UpdateProductPriceAsync(int id, decimal newPrice);

        // Search and Filter
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);

        // Validation
        Task<bool> CanDeleteCategoryAsync(int categoryId);

        // DTO-based methods for better API integration
        Task<Product> CreateProductFromDtoAsync(CreateProductRequest request);
        Task<Product> UpdateProductFromDtoAsync(int id, UpdateProductRequest request);

        // Story 2.3: Quick Operations
        Task UpdateProductAvailabilityAsync(int id, bool isActive);
        Task<BulkOperationResult> BulkUpdateProductsAsync(BulkUpdateRequest request);
        Task<IEnumerable<object>> GetProductPriceHistoryAsync(int productId);
        Task LogProductActivityAsync(int productId, string action, string? previousValue, string? newValue, string? userId = null);
    }
}
