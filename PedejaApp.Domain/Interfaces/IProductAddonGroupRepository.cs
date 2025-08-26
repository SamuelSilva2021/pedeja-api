using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

public interface IProductAddonGroupRepository
{
    Task<ProductAddonGroup?> GetByIdAsync(int id);
    Task<IEnumerable<ProductAddonGroup>> GetByProductIdAsync(int productId);
    Task<ProductAddonGroup?> GetByProductAndAddonGroupAsync(int productId, int addonGroupId);
    Task<ProductAddonGroup> AddAsync(ProductAddonGroup productAddonGroup);
    Task<ProductAddonGroup> UpdateAsync(ProductAddonGroup productAddonGroup);
    Task DeleteAsync(ProductAddonGroup productAddonGroup);
    Task<IEnumerable<ProductAddonGroup>> AddRangeAsync(IEnumerable<ProductAddonGroup> productAddonGroups);
    Task DeleteRangeAsync(IEnumerable<ProductAddonGroup> productAddonGroups);
    Task<bool> ExistsAsync(int productId, int addonGroupId);
    Task<IEnumerable<Product>> GetProductsByAddonGroupAsync(int addonGroupId);
    Task SaveChangesAsync();
}
