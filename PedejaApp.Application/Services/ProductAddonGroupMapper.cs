using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.DTOs;

namespace PedejaApp.Application.Services;

/// <summary>
/// Implementação do mapeador de grupos de adicionais de produtos usando recursos modernos do C# 13
/// </summary>
public class ProductAddonGroupMapper : IProductAddonGroupMapper
{
    /// <summary>
    /// Mapeia uma entidade ProductAddonGroup para ProductAddonGroupResponseDto
    /// </summary>
    public ProductAddonGroupResponseDto MapToDto(ProductAddonGroup productAddonGroup)
    {
        ArgumentNullException.ThrowIfNull(productAddonGroup);
        
        return new ProductAddonGroupResponseDto
        {
            Id = productAddonGroup.Id,
            ProductId = productAddonGroup.ProductId,
            AddonGroupId = productAddonGroup.AddonGroupId,
            IsRequired = productAddonGroup.IsRequired,
            MaxSelectionsOverride = productAddonGroup.MaxSelectionsOverride,
            MinSelectionsOverride = productAddonGroup.MinSelectionsOverride,
            DisplayOrder = productAddonGroup.DisplayOrder,
            AddonGroup = productAddonGroup.AddonGroup
            != null ? MapToAddonGroupDto(productAddonGroup.AddonGroup) : null
        };
    }

    /// <summary>
    /// Mapeia uma coleção de entidades ProductAddonGroup para ProductAddonGroupResponseDto usando collection expressions
    /// </summary>
    public IEnumerable<ProductAddonGroupResponseDto> MapToDtos(IEnumerable<ProductAddonGroup> productAddonGroups)
    {
        ArgumentNullException.ThrowIfNull(productAddonGroups);
        
        return productAddonGroups.Select(MapToDto);
    }

    /// <summary>
    /// Mapeia um AddProductAddonGroupRequestDto para entidade ProductAddonGroup
    /// </summary>
    public ProductAddonGroup MapToEntity(AddProductAddonGroupRequestDto request, int productId)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        return new ProductAddonGroup
        {
            ProductId = productId,
            AddonGroupId = request.AddonGroupId,
            IsRequired = request.IsRequired,
            MaxSelectionsOverride = request.MaxSelectionsOverride,
            MinSelectionsOverride = request.MinSelectionsOverride,
            DisplayOrder = request.DisplayOrder
        };
    }

    /// <summary>
    /// Atualiza uma entidade ProductAddonGroup com dados do UpdateProductAddonGroupRequestDto
    /// </summary>
    public void MapToEntity(UpdateProductAddonGroupRequestDto request, ProductAddonGroup productAddonGroup)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(productAddonGroup);
        
        productAddonGroup.IsRequired = request.IsRequired;
        productAddonGroup.MaxSelectionsOverride = request.MaxSelectionsOverride;
        productAddonGroup.MinSelectionsOverride = request.MinSelectionsOverride;
        productAddonGroup.DisplayOrder = request.DisplayOrder;
    }

    /// <summary>
    /// Mapeia uma entidade Product com seus grupos de adicionais para ProductWithAddonsResponseDto
    /// </summary>
    public ProductWithAddonsResponseDto MapToProductWithAddonsDto(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        return new ProductWithAddonsResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            AddonGroups = product.AddonGroups?.Select(pag => new AddonGroupResponseDto
            {
                Id = pag.AddonGroup?.Id ?? 0,
                Name = pag.AddonGroup?.Name ?? string.Empty,
                Description = pag.AddonGroup?.Description,
                IsRequired = pag.IsRequired,
                MaxSelections = pag.MaxSelectionsOverride ?? pag.AddonGroup?.MaxSelections ?? 0,
                MinSelections = pag.MinSelectionsOverride ?? pag.AddonGroup?.MinSelections ?? 0,
                DisplayOrder = pag.DisplayOrder,
                Addons = pag.AddonGroup?.Addons?.Select(MapToAddonDto).ToList() ?? []
            }).ToList() ?? []
        };
    }

    /// <summary>
    /// Mapeia uma entidade AddonGroup para AddonGroupResponseDto
    /// </summary>
    public AddonGroupResponseDto MapToAddonGroupDto(AddonGroup addonGroup)
    {
        ArgumentNullException.ThrowIfNull(addonGroup);
        
        return new AddonGroupResponseDto
        {
            Id = addonGroup.Id,
            Name = addonGroup.Name,
            Description = addonGroup.Description,
            IsRequired = false, // Será definido pelo ProductAddonGroup
            MaxSelections = 0,  // Será definido pelo ProductAddonGroup
            MinSelections = 0,  // Será definido pelo ProductAddonGroup
            DisplayOrder = 0,   // Será definido pelo ProductAddonGroup
            Addons = addonGroup.Addons?.Select(MapToAddonDto).ToList() ?? []
        };
    }

    /// <summary>
    /// Mapeia uma entidade Addon para AddonResponseDto
    /// </summary>
    public AddonResponseDto MapToAddonDto(Addon addon)
    {
        ArgumentNullException.ThrowIfNull(addon);
        
        return new AddonResponseDto
        {
            Id = addon.Id,
            Name = addon.Name,
            Description = addon.Description,
            Price = addon.Price,
            IsActive = addon.IsActive,
            DisplayOrder = addon.DisplayOrder
        };
    }
}