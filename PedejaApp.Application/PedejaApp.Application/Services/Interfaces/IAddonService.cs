using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services.Interfaces;

public interface IAddonService
{
    // CRUD Operations
    Task<IEnumerable<Addon>> GetAllAddonsAsync();
    Task<IEnumerable<Addon>> GetActiveAddonsAsync();
    Task<IEnumerable<Addon>> GetByAddonGroupIdAsync(int addonGroupId);
    Task<IEnumerable<Addon>> GetActiveByAddonGroupIdAsync(int addonGroupId);
    Task<Addon?> GetAddonByIdAsync(int id);
    Task<Addon> CreateAddonAsync(CreateAddonRequestDto request);
    Task<Addon> UpdateAddonAsync(int id, UpdateAddonRequestDto request);
    Task DeleteAddonAsync(int id);
    
    // Business Operations
    Task<Addon> ToggleAddonStatusAsync(int id);
    Task ReorderAddonsAsync(Dictionary<int, int> addonOrders);
    Task<Addon> UpdateAddonPriceAsync(int id, decimal newPrice);
    
    // Validation
    Task<bool> IsNameUniqueInGroupAsync(string name, int addonGroupId, int? excludeId = null);
    Task<bool> CanDeleteAddonAsync(int id);
}