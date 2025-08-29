using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services;

public class AddonService : IAddonService
{
    private readonly IAddonRepository _addonRepository;
    private readonly IAddonGroupRepository _addonGroupRepository;

    public AddonService(IAddonRepository addonRepository, IAddonGroupRepository addonGroupRepository)
    {
        _addonRepository = addonRepository;
        _addonGroupRepository = addonGroupRepository;
    }

    public async Task<IEnumerable<Addon>> GetAllAddonsAsync() =>
        await _addonRepository.GetAllAsync();

    public async Task<IEnumerable<Addon>> GetActiveAddonsAsync() =>
        await _addonRepository.GetActiveAddonsAsync();

    public async Task<IEnumerable<Addon>> GetByAddonGroupIdAsync(int addonGroupId) => 
        await _addonRepository.GetByAddonGroupIdAsync(addonGroupId);

    public async Task<IEnumerable<Addon>> GetActiveByAddonGroupIdAsync(int addonGroupId) =>
        await _addonRepository.GetActiveByAddonGroupIdAsync(addonGroupId);

    public async Task<Addon?> GetAddonByIdAsync(int id) =>
        await _addonRepository.GetByIdAsync(id);

    public async Task<Addon> CreateAddonAsync(CreateAddonRequestDto request)
    {
        // Validar se o grupo existe
        var addonGroup = await _addonGroupRepository.GetByIdAsync(request.AddonGroupId);
        if (addonGroup == null)
            throw new ArgumentException("Grupo de adicionais não encontrado");

        // Validar nome único no grupo
        if (!await _addonRepository.IsNameUniqueInGroupAsync(request.Name, request.AddonGroupId))
            throw new ArgumentException("Já existe um adicional com este nome neste grupo");

        var addon = new Addon
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            AddonGroupId = request.AddonGroupId,
            DisplayOrder = request.DisplayOrder > 0 ? request.DisplayOrder : await _addonRepository.GetNextDisplayOrderAsync(request.AddonGroupId),
            IsActive = request.IsActive,
            ImageUrl = request.ImageUrl?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _addonRepository.AddAsync(addon);
    }

    public async Task<Addon> UpdateAddonAsync(int id, UpdateAddonRequestDto request)
    {
        var addon = await _addonRepository.GetByIdAsync(id);
        if (addon == null)
            throw new ArgumentException("Adicional não encontrado");

        // Validar se o novo grupo de adicionais existe
        var addonGroup = await _addonGroupRepository.GetByIdAsync(request.AddonGroupId);
        if (addonGroup == null)
            throw new ArgumentException("Grupo de adicionais não encontrado");

        // Validar nome único no NOVO grupo (se o grupo mudou) ou no grupo atual
        if (!await _addonRepository.IsNameUniqueInGroupAsync(request.Name, request.AddonGroupId, id))
            throw new ArgumentException("Já existe um adicional com este nome neste grupo");

        addon.Name = request.Name.Trim();
        addon.Description = request.Description?.Trim();
        addon.Price = request.Price;
        addon.AddonGroupId = request.AddonGroupId; // *** ESTA LINHA ESTAVA FALTANDO ***
        addon.DisplayOrder = request.DisplayOrder;
        addon.IsActive = request.IsActive;
        addon.ImageUrl = request.ImageUrl?.Trim();
        addon.UpdatedAt = DateTime.UtcNow;

        await _addonRepository.UpdateAsync(addon);
        return addon;
    }

    public async Task DeleteAddonAsync(int id)
    {
        if (!await CanDeleteAddonAsync(id))
            throw new InvalidOperationException("Não é possível excluir este adicional pois ele está sendo usado em pedidos");

        await _addonRepository.DeleteVirtualAsync(id);
    }

    public async Task<Addon> ToggleAddonStatusAsync(int id)
    {
        var addon = await _addonRepository.GetByIdAsync(id);
        if (addon == null)
            throw new ArgumentException("Adicional não encontrado");

        addon.IsActive = !addon.IsActive;
        addon.UpdatedAt = DateTime.UtcNow;

        await _addonRepository.UpdateAsync(addon);
        return addon;
    }

    public async Task ReorderAddonsAsync(Dictionary<int, int> addonOrders) =>
        await _addonRepository.UpdateDisplayOrdersAsync(addonOrders);
    

    public async Task<Addon> UpdateAddonPriceAsync(int id, decimal newPrice)
    {
        var addon = await _addonRepository.GetByIdAsync(id);
        if (addon == null)
            throw new ArgumentException("Adicional não encontrado");

        addon.Price = newPrice;
        addon.UpdatedAt = DateTime.UtcNow;

        await _addonRepository.UpdateAsync(addon);
        return addon;
    }

    public async Task<bool> IsNameUniqueInGroupAsync(string name, int addonGroupId, int? excludeId = null) =>
        await _addonRepository.IsNameUniqueInGroupAsync(name, addonGroupId, excludeId);

    public Task<bool> CanDeleteAddonAsync(int id)
    {
        //TODO:
        // Verificar se o adicional está sendo usado em algum pedido
        // Esta lógica pode ser implementada conforme necessário
        return Task.FromResult(true);
    }
}