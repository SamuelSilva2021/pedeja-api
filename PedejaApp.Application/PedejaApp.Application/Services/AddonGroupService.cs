using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services;

public class AddonGroupService : IAddonGroupService
{
    private readonly IAddonGroupRepository _addonGroupRepository;
    private readonly IProductRepository _productRepository;

    public AddonGroupService(IAddonGroupRepository addonGroupRepository, IProductRepository productRepository)
    {
        _addonGroupRepository = addonGroupRepository;
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<AddonGroup>> GetAllAddonGroupsAsync() =>
        await _addonGroupRepository.GetAllAsync();

    public async Task<IEnumerable<AddonGroup>> GetActiveAddonGroupsAsync() =>
        await _addonGroupRepository.GetActiveAddonGroupsAsync();

    public async Task<AddonGroup?> GetAddonGroupByIdAsync(int id) =>
        await _addonGroupRepository.GetByIdAsync(id);
    

    public async Task<AddonGroup?> GetAddonGroupWithAddonsAsync(int id) =>
        await _addonGroupRepository.GetWithAddonsAsync(id);

    public async Task<IEnumerable<AddonGroup>> GetByProductIdAsync(int productId) =>
        await _addonGroupRepository.GetByProductIdAsync(productId);

    public async Task<AddonGroup> CreateAddonGroupAsync(CreateAddonGroupRequestDto request)
    {
        // Validar nome único
        if (!await _addonGroupRepository.IsNameUniqueAsync(request.Name))
            throw new ArgumentException("Já existe um grupo de adicionais com este nome");

        var addonGroup = new AddonGroup
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Type = request.Type,
            MinSelections = request.MinSelections,
            MaxSelections = request.MaxSelections,
            IsRequired = request.IsRequired,
            DisplayOrder = request.DisplayOrder > 0 ? request.DisplayOrder : await _addonGroupRepository.GetNextDisplayOrderAsync(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _addonGroupRepository.AddAsync(addonGroup);
    }

    public async Task<AddonGroup> UpdateAddonGroupAsync(int id, UpdateAddonGroupRequestDto request)
    {
        var addonGroup = await _addonGroupRepository.GetByIdAsync(id);
        if (addonGroup == null)
            throw new ArgumentException("Grupo de adicionais não encontrado");

        // Validar nome único
        if (!await _addonGroupRepository.IsNameUniqueAsync(request.Name, id))
            throw new ArgumentException("Já existe um grupo de adicionais com este nome");

        addonGroup.Name = request.Name.Trim();
        addonGroup.Description = request.Description?.Trim();
        addonGroup.Type = request.Type;
        addonGroup.MinSelections = request.MinSelections;
        addonGroup.MaxSelections = request.MaxSelections;
        addonGroup.IsRequired = request.IsRequired;
        addonGroup.DisplayOrder = request.DisplayOrder;
        addonGroup.IsActive = request.IsActive;
        addonGroup.UpdatedAt = DateTime.UtcNow;

        await _addonGroupRepository.UpdateAsync(addonGroup);
        return addonGroup;
    }

    public async Task DeleteAddonGroupAsync(int id)
    {
        if (!await CanDeleteAddonGroupAsync(id))
            throw new InvalidOperationException("Não é possível excluir este grupo pois ele possui adicionais ou está associado a produtos");

        await _addonGroupRepository.DeleteAsync(id);
    }

    public async Task<AddonGroup> ToggleAddonGroupStatusAsync(int id)
    {
        var addonGroup = await _addonGroupRepository.GetByIdAsync(id);
        if (addonGroup == null)
            throw new ArgumentException("Grupo de adicionais não encontrado");

        addonGroup.IsActive = !addonGroup.IsActive;
        addonGroup.UpdatedAt = DateTime.UtcNow;

        await _addonGroupRepository.UpdateAsync(addonGroup);
        return addonGroup;
    }

    public async Task ReorderAddonGroupsAsync(Dictionary<int, int> groupOrders) => 
        await _addonGroupRepository.UpdateDisplayOrdersAsync(groupOrders);

    public async Task<ProductAddonGroup> AssignToProductAsync(int productId, int addonGroupId, AssignAddonGroupToProductRequestDto request)
    {
        // Verificar se produto existe
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new ArgumentException("Produto não encontrado");

        // Verificar se grupo existe
        var addonGroup = await _addonGroupRepository.GetByIdAsync(addonGroupId);
        if (addonGroup == null)
            throw new ArgumentException("Grupo de adicionais não encontrado");

        var productAddonGroup = new ProductAddonGroup
        {
            ProductId = productId,
            AddonGroupId = addonGroupId,
            DisplayOrder = request.DisplayOrder,
            IsRequired = request.IsRequired,
            MinSelectionsOverride = request.MinSelectionsOverride,
            MaxSelectionsOverride = request.MaxSelectionsOverride
        };
        return productAddonGroup;
    }

    public async Task RemoveFromProductAsync(int productId, int addonGroupId)
    {
        //TODO:
        // Implementar remoção da associação produto-grupo
        // Esta implementação precisaria de um repositório específico para ProductAddonGroup
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null) => 
        await _addonGroupRepository.IsNameUniqueAsync(name, excludeId);

    public async Task<bool> CanDeleteAddonGroupAsync(int id) => 
        !await _addonGroupRepository.HasAddonsAsync(id);
}