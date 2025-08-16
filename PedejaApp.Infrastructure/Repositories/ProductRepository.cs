using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Infrastructure.Data;

namespace PedejaApp.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Category.DisplayOrder)
                .ThenBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Category.IsActive)
                .OrderBy(p => p.Category.DisplayOrder)
                .ThenBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            // Set display order if not specified
            if (product.DisplayOrder == 0)
            {
                product.DisplayOrder = await GetNextDisplayOrderAsync(product.CategoryId);
            }

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            
            // Reload with Category
            return await GetByIdAsync(product.Id) ?? product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            var existingProduct = await _context.Products.FindAsync(product.Id);
            if (existingProduct == null)
                throw new ArgumentException($"Product with ID {product.Id} not found");

            // Update properties
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.IsActive = product.IsActive;
            existingProduct.DisplayOrder = product.DisplayOrder;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Reload with Category
            return await GetByIdAsync(product.Id) ?? existingProduct;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetProductsForMenuAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Category.IsActive)
                .OrderBy(p => p.Category.DisplayOrder)
                .ThenBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<int> GetNextDisplayOrderAsync(int categoryId)
        {
            var maxOrder = await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .MaxAsync(p => (int?)p.DisplayOrder);

            return (maxOrder ?? 0) + 1;
        }

        public async Task ReorderProductsAsync(Dictionary<int, int> productOrders)
        {
            foreach (var kvp in productOrders)
            {
                var product = await _context.Products.FindAsync(kvp.Key);
                if (product != null)
                {
                    product.DisplayOrder = kvp.Value;
                    product.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasProductsInCategoryAsync(int categoryId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            searchTerm = searchTerm.ToLower();

            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.ToLower().Contains(searchTerm) ||
                           (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                           p.Category.Name.ToLower().Contains(searchTerm))
                .OrderBy(p => p.Category.DisplayOrder)
                .ThenBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .OrderBy(p => p.Price)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }
    }
}
