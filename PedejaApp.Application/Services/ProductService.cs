using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Domain.DTOs;

namespace PedejaApp.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
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

        public async Task<Product?> GetProductByIdAsync(int id) => await _productRepository.GetByIdAsync(id);

        public async Task<Product> CreateProductAsync(Product product)
        {
            // Validate category exists and is active
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            if (category == null)
                throw new ArgumentException("Category not found");

            if (!category.IsActive)
                throw new ArgumentException("Cannot add products to inactive category");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Product name is required");

            if (product.Price <= 0)
                throw new ArgumentException("Product price must be greater than zero");

            // Validate name uniqueness using search
            var existingProducts = await _productRepository.SearchProductsAsync(product.Name);
            if (existingProducts.Any(p => string.Equals(p.Name, product.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Product with this name already exists");

            // Set display order if not specified
            if (product.DisplayOrder == 0)
            {
                product.DisplayOrder = await _productRepository.GetNextDisplayOrderAsync(product.CategoryId);
            }

            return await _productRepository.AddAsync(product);
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            // Validate product exists
            var existingProduct = await _productRepository.GetByIdAsync(product.Id);
            if (existingProduct == null)
                throw new ArgumentException("Product not found");

            // Validate category exists and is active
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            if (category == null)
                throw new ArgumentException("Category not found");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Product name is required");

            if (product.Price <= 0)
                throw new ArgumentException("Product price must be greater than zero");

            // Validate name uniqueness if changed
            if (!string.Equals(product.Name, existingProduct.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existingProducts = await _productRepository.SearchProductsAsync(product.Name);
                var duplicateProduct = existingProducts.FirstOrDefault(p => 
                    string.Equals(p.Name, product.Name, StringComparison.OrdinalIgnoreCase) && p.Id != product.Id);
                
                if (duplicateProduct != null)
                    throw new InvalidOperationException("Product with this name already exists");
            }

            await _productRepository.UpdateAsync(product);
            return product;
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new ArgumentException("Product not found");

            await _productRepository.DeleteAsync(product);
        }

        public async Task<IEnumerable<Product>> GetProductsForMenuAsync()
        {
            return await _productRepository.GetProductsForMenuAsync();
        }

        public async Task ReorderProductsAsync(Dictionary<int, int> productOrders)
        {
            // Validate all products exist
            foreach (var productId in productOrders.Keys)
            {
                if (!await _productRepository.ExistsAsync(productId))
                    throw new ArgumentException($"Product with ID {productId} not found");
            }

            await _productRepository.ReorderProductsAsync(productOrders);
        }

        public async Task<Product> ToggleProductStatusAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new ArgumentException("Produto não encontrado");

            product.IsActive = !product.IsActive;
            await _productRepository.UpdateAsync(product);
            return product;
        }

        public async Task<Product> UpdateProductPriceAsync(int id, decimal newPrice)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new ArgumentException("Product not found");

            if (newPrice <= 0)
                throw new ArgumentException("Price must be greater than zero");

            product.Price = newPrice;
            await _productRepository.UpdateAsync(product);
            return product;
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _productRepository.SearchProductsAsync(searchTerm);
        }

        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
                throw new ArgumentException("Invalid price range");

            return await _productRepository.GetProductsByPriceRangeAsync(minPrice, maxPrice);
        }

        public async Task<bool> CanDeleteCategoryAsync(int categoryId)
        {
            return !await _productRepository.HasProductsInCategoryAsync(categoryId);
        }

        public async Task<Product> CreateProductFromDtoAsync(CreateProductRequestDto request)
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

            // Update properties from DTO
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

        public async Task<IEnumerable<object>> GetProductPriceHistoryAsync(int productId)
        {
            await Task.CompletedTask;
            return new List<object>();
        }

        public async Task LogProductActivityAsync(int productId, string action, string? previousValue, string? newValue, string? userId = null)
        {
            await Task.CompletedTask;
        }

        public async Task<PedejaApp.Domain.DTOs.BulkOperationResult> BulkUpdateProductsAsync(PedejaApp.Domain.DTOs.BulkUpdateRequest request)
        {
            await Task.CompletedTask;
            return new PedejaApp.Domain.DTOs.BulkOperationResult { SuccessCount = 0, FailureCount = 0, Errors = new List<string>() };
        }

        public async Task UpdateProductAvailabilityAsync(int id, bool isActive)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                product.IsActive = isActive;
                await _productRepository.UpdateAsync(product);
            }
        }
    }
}
