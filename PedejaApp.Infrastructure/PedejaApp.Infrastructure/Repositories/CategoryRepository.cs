using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Infrastructure.Data;

namespace PedejaApp.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
    {
        return await _dbSet
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetCategoriesOrderedAsync()
    {
        return await _dbSet
            .Include(x => x.Products.Where(p => !p.IsDeleted))
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();
    }

    public async Task<bool> HasProductsAsync(int categoryId)
    {
        return await _context.Products
            .AnyAsync(x => x.CategoryId == categoryId && !x.IsDeleted && x.IsActive);
    }

    public async Task<int> GetNextDisplayOrderAsync()
    {
        var maxOrder = await _dbSet
            .Where(x => !x.IsDeleted)
            .MaxAsync(x => (int?)x.DisplayOrder);
        
        return (maxOrder ?? 0) + 1;
    }

    public async Task UpdateDisplayOrdersAsync(Dictionary<int, int> categoryDisplayOrders)
    {
        foreach (var kvp in categoryDisplayOrders)
        {
            var category = await _dbSet.FindAsync(kvp.Key);
            if (category != null && !category.IsDeleted)
            {
                category.DisplayOrder = kvp.Value;
                category.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
    {
        var query = _dbSet.Where(x => !x.IsDeleted && x.Name.ToLower() == name.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
