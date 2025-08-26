using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

public interface IAddonRepository : IRepository<Addon>
{
    Task<IEnumerable<Addon>> GetByAddonGroupIdAsync(int addonGroupId);
    Task<IEnumerable<Addon>> GetActiveByAddonGroupIdAsync(int addonGroupId);
    Task<bool> IsNameUniqueInGroupAsync(string name, int addonGroupId, int? excludeId = null);
    Task<int> GetNextDisplayOrderAsync(int addonGroupId);
    Task UpdateDisplayOrdersAsync(Dictionary<int, int> addonDisplayOrders);
    Task<IEnumerable<Addon>> GetActiveAddonsAsync();
}