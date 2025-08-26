using PedejaApp.Application.Common.Models;
using PedejaApp.Domain.DTOs;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services.Interfaces;

/// <summary>
/// Interface para validações de negócio de produtos
/// </summary>
public interface IProductValidationService
{
    /// <summary>
    /// Valida se uma categoria existe e está ativa
    /// </summary>
    /// <param name="categoryId">ID da categoria</param>
    /// <returns>True se a categoria é válida</returns>
    Task<bool> IsCategoryValidAsync(int categoryId);
    
    /// <summary>
    /// Valida se o nome do produto é único (excluindo o próprio produto na atualização)
    /// </summary>
    /// <param name="name">Nome do produto</param>
    /// <param name="excludeProductId">ID do produto a ser excluído da validação (para atualização)</param>
    /// <returns>True se o nome é único</returns>
    Task<bool> IsProductNameUniqueAsync(string name, int? excludeProductId = null);
    
    /// <summary>
    /// Valida se um produto pode ser excluído (não está em pedidos ativos)
    /// </summary>
    /// <param name="productId">ID do produto</param>
    /// <returns>True se o produto pode ser excluído</returns>
    Task<bool> CanDeleteProductAsync(int productId);
    
    /// <summary>
    /// Valida dados de criação de produto
    /// </summary>
    /// <param name="request">Request de criação</param>
    /// <returns>Lista de erros de validação</returns>
    Task<IEnumerable<string>> ValidateCreateProductAsync(CreateProductRequestDto request);
    
    /// <summary>
    /// Valida dados de atualização de produto
    /// </summary>
    /// <param name="productId">ID do produto</param>
    /// <param name="request">Request de atualização</param>
    /// <returns>Lista de erros de validação</returns>
    Task<IEnumerable<string>> ValidateUpdateProductAsync(int productId, UpdateProductRequest request);
    
    /// <summary>
    /// Valida dados de criação de produto com resultado estruturado
    /// </summary>
    /// <param name="request">Request de criação</param>
    /// <returns>Resultado da validação</returns>
    Task<ValidationResult> ValidateCreateProductRequestAsync(CreateProductRequestDto request);
    
    /// <summary>
    /// Valida dados de atualização de produto com resultado estruturado
    /// </summary>
    /// <param name="productId">ID do produto</param>
    /// <param name="request">Request de atualização</param>
    /// <returns>Resultado da validação</returns>
    Task<ValidationResult> ValidateUpdateProductRequestAsync(int productId, UpdateProductRequest request);
}