using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Infrastructure.Data;

namespace PedejaApp.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync() =>
        await _dbSet
            .Include(p => p.Category)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetActiveProductsByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .Include(p => p.Category)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
    }

    public async Task<int> GetNextDisplayOrderAsync(int categoryId)
    {
        var maxOrder = await _dbSet
            .Where(p => p.CategoryId == categoryId)
            .MaxAsync(p => (int?)p.DisplayOrder);
        
        return (maxOrder ?? 0) + 1;
    }

    public async Task UpdateDisplayOrdersAsync(Dictionary<int, int> productDisplayOrders)
    {
        foreach (var item in productDisplayOrders)
        {
            var product = await _dbSet.FindAsync(item.Key);
            if (product != null)
            {
                product.DisplayOrder = item.Value;
                product.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsNameUniqueInCategoryAsync(string name, int categoryId, int? excludeId = null)
    {
        var query = _dbSet.Where(p => 
            p.Name.ToLower() == name.ToLower() && 
            p.CategoryId == categoryId);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}
