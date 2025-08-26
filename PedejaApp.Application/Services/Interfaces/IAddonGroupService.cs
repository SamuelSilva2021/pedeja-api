using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services.Interfaces;

public interface IAddonGroupService
{
    // CRUD Operations
    Task<IEnumerable<AddonGroup>> GetAllAddonGroupsAsync();
    Task<IEnumerable<AddonGroup>> GetActiveAddonGroupsAsync();
    Task<AddonGroup?> GetAddonGroupByIdAsync(int id);
    Task<AddonGroup?> GetAddonGroupWithAddonsAsync(int id);
    Task<IEnumerable<AddonGroup>> GetByProductIdAsync(int productId);
    Task<AddonGroup> CreateAddonGroupAsync(CreateAddonGroupRequestDto request);
    Task<AddonGroup> UpdateAddonGroupAsync(int id, UpdateAddonGroupRequestDto request);
    Task DeleteAddonGroupAsync(int id);
    
    // Business Operations
    Task<AddonGroup> ToggleAddonGroupStatusAsync(int id);
    Task ReorderAddonGroupsAsync(Dictionary<int, int> groupOrders);
    Task<ProductAddonGroup> AssignToProductAsync(int productId, int addonGroupId, AssignAddonGroupToProductRequestDto request);
    Task RemoveFromProductAsync(int productId, int addonGroupId);
    
    // Validation
    Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    Task<bool> CanDeleteAddonGroupAsync(int id);
}