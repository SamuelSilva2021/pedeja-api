using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Infrastructure.Data;

namespace PedejaApp.Infrastructure.Repositories;

public class AddonGroupRepository : Repository<AddonGroup>, IAddonGroupRepository
{
    public AddonGroupRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AddonGroup>> GetActiveAddonGroupsAsync()
    {
        return await _dbSet
            .Where(ag => ag.IsActive && !ag.IsDeleted)
            .Include(ag => ag.Addons.Where(a => a.IsActive && !a.IsDeleted))
            .OrderBy(ag => ag.DisplayOrder)
            .ThenBy(ag => ag.Name)
            .ToListAsync();
    }

    public async Task<AddonGroup?> GetWithAddonsAsync(int id)
    {
        return await _dbSet
            .Where(ag => ag.Id == id && !ag.IsDeleted)
            .Include(ag => ag.Addons.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AddonGroup>> GetByProductIdAsync(int productId)
    {
        return await _context.ProductAddonGroups
            .Where(pag => pag.ProductId == productId)
            .Include(pag => pag.AddonGroup)
                .ThenInclude(ag => ag.Addons.Where(a => a.IsActive && !a.IsDeleted))
            .Where(pag => pag.AddonGroup.IsActive && !pag.AddonGroup.IsDeleted)
            .OrderBy(pag => pag.DisplayOrder)
            .Select(pag => pag.AddonGroup)
            .ToListAsync();
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
    {
        var query = _dbSet.Where(ag => ag.Name.ToLower() == name.ToLower() && !ag.IsDeleted);
        
        if (excludeId.HasValue)
            query = query.Where(ag => ag.Id != excludeId.Value);
            
        return !await query.AnyAsync();
    }

    public async Task<int> GetNextDisplayOrderAsync()
    {
        var maxOrder = await _dbSet
            .Where(ag => !ag.IsDeleted)
            .MaxAsync(ag => (int?)ag.DisplayOrder);
            
        return (maxOrder ?? 0) + 1;
    }

    public async Task UpdateDisplayOrdersAsync(Dictionary<int, int> groupDisplayOrders)
    {
        foreach (var kvp in groupDisplayOrders)
        {
            var group = await _dbSet.FindAsync(kvp.Key);
            if (group != null && !group.IsDeleted)
            {
                group.DisplayOrder = kvp.Value;
                group.UpdatedAt = DateTime.UtcNow;
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasAddonsAsync(int addonGroupId)
    {
        return await _context.Addons
            .AnyAsync(a => a.AddonGroupId == addonGroupId && !a.IsDeleted);
    }
}