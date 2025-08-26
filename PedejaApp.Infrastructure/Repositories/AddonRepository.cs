using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Infrastructure.Data;

namespace PedejaApp.Infrastructure.Repositories;

public class AddonRepository : Repository<Addon>, IAddonRepository
{
    public AddonRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Addon>> GetByAddonGroupIdAsync(int addonGroupId)
    {
        return await _dbSet
            .Where(a => a.AddonGroupId == addonGroupId)
            .Include(a => a.AddonGroup)
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Addon>> GetActiveByAddonGroupIdAsync(int addonGroupId)
    {
        return await _dbSet
            .Where(a => a.AddonGroupId == addonGroupId && a.IsActive)
            .Include(a => a.AddonGroup)
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<bool> IsNameUniqueInGroupAsync(string name, int addonGroupId, int? excludeId = null)
    {
        var query = _dbSet.Where(a => a.Name.ToLower() == name.ToLower() && 
                                     a.AddonGroupId == addonGroupId);
        
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
            
        return !await query.AnyAsync();
    }

    public async Task<int> GetNextDisplayOrderAsync(int addonGroupId)
    {
        var maxOrder = await _dbSet
            .Where(a => a.AddonGroupId == addonGroupId)
            .MaxAsync(a => (int?)a.DisplayOrder);
            
        return (maxOrder ?? 0) + 1;
    }

    public async Task UpdateDisplayOrdersAsync(Dictionary<int, int> addonDisplayOrders)
    {
        foreach (var kvp in addonDisplayOrders)
        {
            var addon = await _dbSet.FindAsync(kvp.Key);
            if (addon != null)
            {
                addon.DisplayOrder = kvp.Value;
                addon.UpdatedAt = DateTime.UtcNow;
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Addon>> GetActiveAddonsAsync()
    {
        return await _dbSet
            .Where(a => a.IsActive)
            .Include(a => a.AddonGroup)
            .OrderBy(a => a.AddonGroup.DisplayOrder)
            .ThenBy(a => a.DisplayOrder)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }
}