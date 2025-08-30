using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.DTOs;

namespace PedejaApp.Application.Services;

/// <summary>
/// Implementação do mapeador de produtos usando recursos modernos do C# 13
/// </summary>
public class ProductMapper(
    IUrlBuilderService urlBuilderService,
    IProductAddonGroupService productAddonGroupService,
    IProductAddonGroupMapper productAddonGroupMapper) : IProductMapper
{
    private readonly IUrlBuilderService _urlBuilderService = urlBuilderService;
    private readonly IProductAddonGroupService _productAddonGroupService = productAddonGroupService;
    private readonly IProductAddonGroupMapper _productAddonGroupMapper = productAddonGroupMapper;

    /// <summary>
    /// Mapeia uma entidade Product para ProductDto
    /// </summary>
    public ProductDto MapToDto(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            ImageUrl = _urlBuilderService.BuildImageUrl(product.ImageUrl),
            IsActive = product.IsActive,
            DisplayOrder = product.DisplayOrder,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            AddonGroups = GetProductAddonGroups(product.Id)
        };
    }

    /// <summary>
    /// Mapeia uma coleção de entidades Product para ProductDto usando collection expressions
    /// </summary>
    public IEnumerable<ProductDto> MapToDtos(IEnumerable<Product> products)
    {
        ArgumentNullException.ThrowIfNull(products);
        
        return products.Select(MapToDto);
    }

    /// <summary>
    /// Mapeia um CreateProductRequestDto para entidade Product
    /// </summary>
    public Product MapToEntity(CreateProductRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var now = DateTime.UtcNow;
        
        return new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            CategoryId = request.CategoryId,
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Atualiza uma entidade Product com dados do UpdateProductRequest
    /// </summary>
    public void MapToEntity(UpdateProductRequest request, Product product)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(product);
        
        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.IsActive = request.IsActive;
        product.DisplayOrder = request.DisplayOrder;
        product.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Obtém os grupos de adicionais de um produto de forma síncrona
    /// </summary>
    private List<ProductAddonGroupResponseDto> GetProductAddonGroups(int productId)
    {
        try
        {
            var addonGroups = _productAddonGroupService.GetProductAddonGroupsAsync(productId).Result;
            return _productAddonGroupMapper.MapToDtos(addonGroups).ToList();
        }
        catch
        {
            // Em caso de erro, retornar lista vazia para não quebrar o fluxo
            return new List<ProductAddonGroupResponseDto>();
        }
    }
}