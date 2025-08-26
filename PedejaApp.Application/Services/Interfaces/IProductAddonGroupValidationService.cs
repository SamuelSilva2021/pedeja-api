using PedejaApp.Application.Common.Models;
using PedejaApp.Domain.DTOs;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services.Interfaces;

/// <summary>
/// Interface para validações de negócio de grupos de adicionais de produtos
/// </summary>
public interface IProductAddonGroupValidationService
{
    /// <summary>
    /// Valida se um produto existe e está ativo
    /// </summary>
    /// <param name="productId">ID do produto</param>
    /// <returns>True se o produto é válido</returns>
    Task<bool> IsProductValidAsync(int productId);
    
    /// <summary>
    /// Valida se um grupo de adicionais existe e está ativo
    /// </summary>
    /// <param name="addonGroupId">ID do grupo de adicionais</param>
    /// <returns>True se o grupo é válido</returns>
    Task<bool> IsAddonGroupValidAsync(int addonGroupId);
    
    /// <summary>
    /// Valida se um grupo de adicionais já está associado a um produto
    /// </summary>
    /// <param name="productId">ID do produto</param>
    /// <param name="addonGroupId">ID do grupo de adicionais</param>
    /// <returns>True se já existe a associação</returns>
    Task<bool> IsAddonGroupAlreadyAssignedAsync(int productId, int addonGroupId);
    
    /// <summary>
    /// Valida se um ProductAddonGroup pode ser excluído (não está em pedidos ativos)
    /// </summary>
    /// <param name="productAddonGroupId">ID do ProductAddonGroup</param>
    /// <returns>True se pode ser excluído</returns>
    Task<bool> CanDeleteProductAddonGroupAsync(int productAddonGroupId);
    
    /// <summary>
    /// Valida dados de adição de grupo de adicionais a produto
    /// </summary>
    /// <param name="productId">ID do produto</param>
    /// <param name="request">Request de adição</param>
    /// <returns>Resultado da validação</returns>
    Task<ValidationResult> ValidateAddAddonGroupToProductAsync(int productId, AddProductAddonGroupRequestDto request);
    
    /// <summary>
    /// Valida dados de atualização de grupo de adicionais de produto
    /// </summary>
    /// <param name="productAddonGroupId">ID do ProductAddonGroup</param>
    /// <param name="request">Request de atualização</param>
    /// <returns>Resultado da validação</returns>
    Task<ValidationResult> ValidateUpdateProductAddonGroupAsync(int productAddonGroupId, UpdateProductAddonGroupRequestDto request);
    
    /// <summary>
    /// Valida dados de adição em lote de grupos de adicionais
    /// </summary>
    /// <param name="productId">ID do produto</param>
    /// <param name="requests">Lista de requests de adição</param>
    /// <returns>Resultado da validação</returns>
    Task<ValidationResult> ValidateBulkAddAddonGroupsAsync(int productId, IEnumerable<AddProductAddonGroupRequestDto> requests);
    
    /// <summary>
    /// Valida se as seleções mínimas e máximas são consistentes
    /// </summary>
    /// <param name="minSelections">Seleções mínimas</param>
    /// <param name="maxSelections">Seleções máximas</param>
    /// <returns>True se são consistentes</returns>
    bool AreSelectionsValid(int? minSelections, int? maxSelections);
    
    /// <summary>
    /// Valida se a ordem de exibição é válida
    /// </summary>
    /// <param name="displayOrder">Ordem de exibição</param>
    /// <returns>True se é válida</returns>
    bool IsDisplayOrderValid(int displayOrder);
}