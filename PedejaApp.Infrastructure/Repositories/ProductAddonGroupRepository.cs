using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Infrastructure.Data;

namespace PedejaApp.Infrastructure.Repositories;

public class ProductAddonGroupRepository : IProductAddonGroupRepository
{
    private readonly ApplicationDbContext _context;

    public ProductAddonGroupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductAddonGroup?> GetByIdAsync(int id)
    {
        return await _context.ProductAddonGroups
            .Include(pag => pag.AddonGroup)
            .Include(pag => pag.Product)
            .FirstOrDefaultAsync(pag => pag.Id == id);
    }

    public async Task<IEnumerable<ProductAddonGroup>> GetByProductIdAsync(int productId)
    {
        return await _context.ProductAddonGroups
            .Include(pag => pag.AddonGroup)
                .ThenInclude(ag => ag.Addons.Where(a => a.IsActive))
            .Where(pag => pag.ProductId == productId)
            .OrderBy(pag => pag.DisplayOrder)
            .ToListAsync();
    }

    public async Task<ProductAddonGroup?> GetByProductAndAddonGroupAsync(int productId, int addonGroupId)
    {
        return await _context.ProductAddonGroups
            .Include(pag => pag.AddonGroup)
            .FirstOrDefaultAsync(pag => pag.ProductId == productId && pag.AddonGroupId == addonGroupId);
    }

    public async Task<ProductAddonGroup> AddAsync(ProductAddonGroup productAddonGroup)
    {
        _context.ProductAddonGroups.Add(productAddonGroup);
        await _context.SaveChangesAsync();

        // Recarregar com navegação
        return await _context.ProductAddonGroups
            .Include(pag => pag.AddonGroup)
            .FirstAsync(pag => pag.Id == productAddonGroup.Id);
    }

    public async Task<ProductAddonGroup> UpdateAsync(ProductAddonGroup productAddonGroup)
    {
        _context.ProductAddonGroups.Update(productAddonGroup);
        await _context.SaveChangesAsync();
        return productAddonGroup;
    }

    public async Task DeleteAsync(ProductAddonGroup productAddonGroup)
    {
        _context.ProductAddonGroups.Remove(productAddonGroup);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProductAddonGroup>> AddRangeAsync(IEnumerable<ProductAddonGroup> productAddonGroups)
    {
        _context.ProductAddonGroups.AddRange(productAddonGroups);
        await _context.SaveChangesAsync();

        // Recarregar com navegação
        var addedIds = productAddonGroups.Select(pag => pag.Id).ToList();
        return await _context.ProductAddonGroups
            .Include(pag => pag.AddonGroup)
            .Where(pag => addedIds.Contains(pag.Id))
            .ToListAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<ProductAddonGroup> productAddonGroups)
    {
        _context.ProductAddonGroups.RemoveRange(productAddonGroups);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int productId, int addonGroupId)
    {
        return await _context.ProductAddonGroups
            .AnyAsync(pag => pag.ProductId == productId && pag.AddonGroupId == addonGroupId);
    }

    public async Task<IEnumerable<Product>> GetProductsByAddonGroupAsync(int addonGroupId)
    {
        return await _context.ProductAddonGroups
            .Include(pag => pag.Product)
            .Where(pag => pag.AddonGroupId == addonGroupId)
            .Select(pag => pag.Product)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
