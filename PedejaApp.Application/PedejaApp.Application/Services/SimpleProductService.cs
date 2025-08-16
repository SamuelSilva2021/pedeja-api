using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Application.Models;

namespace PedejaApp.Application.Services
{
    public class SimpleProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public SimpleProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _productRepository.GetActiveProductsAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _productRepository.GetProductsByCategoryAsync(categoryId);
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            return await _productRepository.AddAsync(product);
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            var existing = await _productRepository.GetByIdAsync(product.Id);
            if (existing == null) throw new ArgumentException("Product not found");
            
            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.CategoryId = product.CategoryId;
            existing.IsActive = product.IsActive;
            existing.ImageUrl = product.ImageUrl;
            existing.UpdatedAt = DateTime.UtcNow;
            
            await _productRepository.UpdateAsync(existing);
            return existing;
        }

        public async Task DeleteProductAsync(int id)
        {
            await _productRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Product>> GetProductsForMenuAsync()
        {
            return await _productRepository.GetActiveProductsAsync();
        }

        public async Task ReorderProductsAsync(Dictionary<int, int> productOrders)
        {
            // Simple implementation - just update display orders
            foreach (var kvp in productOrders)
            {
                var product = await _productRepository.GetByIdAsync(kvp.Key);
                if (product != null)
                {
                    product.DisplayOrder = kvp.Value;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _productRepository.UpdateAsync(product);
                }
            }
        }

        public async Task<Product> ToggleProductStatusAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) throw new ArgumentException("Product not found");
            
            product.IsActive = !product.IsActive;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);
            return product;
        }

        public async Task<Product> UpdateProductPriceAsync(int id, decimal newPrice)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) throw new ArgumentException("Product not found");
            
            product.Price = newPrice;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);
            return product;
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            var products = await _productRepository.GetActiveProductsAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
                return products;
                
            return products.Where(p => 
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var products = await _productRepository.GetActiveProductsAsync();
            return products.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
        }

        public async Task<bool> CanDeleteCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
            return !products.Any(p => p.IsActive && !p.IsDeleted);
        }

        public async Task<Product> CreateProductFromDtoAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                ImageUrl = request.ImageUrl,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await CreateProductAsync(product);
        }

        public async Task<Product> UpdateProductFromDtoAsync(int id, UpdateProductRequest request)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
                throw new ArgumentException("Product not found");

            existingProduct.Name = request.Name;
            existingProduct.Description = request.Description;
            existingProduct.Price = request.Price;
            existingProduct.CategoryId = request.CategoryId;
            existingProduct.ImageUrl = request.ImageUrl;
            existingProduct.IsActive = request.IsActive;
            existingProduct.DisplayOrder = request.DisplayOrder;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            return await UpdateProductAsync(existingProduct);
        }
    }
}
