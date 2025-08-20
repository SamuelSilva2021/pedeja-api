using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

public interface IAddonGroupRepository : IRepository<AddonGroup>
{
    Task<IEnumerable<AddonGroup>> GetActiveAddonGroupsAsync();
    Task<AddonGroup?> GetWithAddonsAsync(int id);
    Task<IEnumerable<AddonGroup>> GetByProductIdAsync(int productId);
    Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    Task<int> GetNextDisplayOrderAsync();
    Task UpdateDisplayOrdersAsync(Dictionary<int, int> groupDisplayOrders);
    Task<bool> HasAddonsAsync(int addonGroupId);
}