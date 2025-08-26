using PedejaApp.Domain.Entities;
using PedejaApp.Domain.DTOs;

namespace PedejaApp.Application.Services.Interfaces;

/// <summary>
/// Interface para mapeamento entre entidades ProductAddonGroup e DTOs
/// </summary>
public interface IProductAddonGroupMapper
{
    /// <summary>
    /// Mapeia uma entidade ProductAddonGroup para ProductAddonGroupResponseDto
    /// </summary>
    /// <param name="productAddonGroup">Entidade ProductAddonGroup</param>
    /// <returns>ProductAddonGroupResponseDto mapeado</returns>
    ProductAddonGroupResponseDto MapToDto(ProductAddonGroup productAddonGroup);
    
    /// <summary>
    /// Mapeia uma coleção de entidades ProductAddonGroup para ProductAddonGroupResponseDto
    /// </summary>
    /// <param name="productAddonGroups">Coleção de entidades ProductAddonGroup</param>
    /// <returns>Coleção de ProductAddonGroupResponseDto mapeados</returns>
    IEnumerable<ProductAddonGroupResponseDto> MapToDtos(IEnumerable<ProductAddonGroup> productAddonGroups);
    
    /// <summary>
    /// Mapeia um AddProductAddonGroupRequestDto para entidade ProductAddonGroup
    /// </summary>
    /// <param name="request">Request de criação</param>
    /// <param name="productId">ID do produto</param>
    /// <returns>Entidade ProductAddonGroup</returns>
    ProductAddonGroup MapToEntity(AddProductAddonGroupRequestDto request, int productId);
    
    /// <summary>
    /// Atualiza uma entidade ProductAddonGroup com dados do UpdateProductAddonGroupRequestDto
    /// </summary>
    /// <param name="request">Request de atualização</param>
    /// <param name="productAddonGroup">Entidade ProductAddonGroup a ser atualizada</param>
    void MapToEntity(UpdateProductAddonGroupRequestDto request, ProductAddonGroup productAddonGroup);
    
    /// <summary>
    /// Mapeia uma entidade Product com seus grupos de adicionais para ProductWithAddonsResponseDto
    /// </summary>
    /// <param name="product">Entidade Product com grupos de adicionais carregados</param>
    /// <returns>ProductWithAddonsResponseDto mapeado</returns>
    ProductWithAddonsResponseDto MapToProductWithAddonsDto(Product product);
    
    /// <summary>
    /// Mapeia uma entidade AddonGroup para AddonGroupResponseDto
    /// </summary>
    /// <param name="addonGroup">Entidade AddonGroup</param>
    /// <returns>AddonGroupResponseDto mapeado</returns>
    AddonGroupResponseDto MapToAddonGroupDto(AddonGroup addonGroup);
    
    /// <summary>
    /// Mapeia uma entidade Addon para AddonResponseDto
    /// </summary>
    /// <param name="addon">Entidade Addon</param>
    /// <returns>AddonResponseDto mapeado</returns>
    AddonResponseDto MapToAddonDto(Addon addon);
}