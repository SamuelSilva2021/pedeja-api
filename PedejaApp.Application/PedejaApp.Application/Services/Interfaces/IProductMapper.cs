using PedejaApp.Domain.Entities;
using PedejaApp.Web.Models.DTOs;
using PedejaApp.Domain.DTOs;

namespace PedejaApp.Application.Services.Interfaces;

/// <summary>
/// Interface para mapeamento entre entidades Product e DTOs
/// </summary>
public interface IProductMapper
{
    /// <summary>
    /// Mapeia uma entidade Product para ProductDto
    /// </summary>
    /// <param name="product">Entidade Product</param>
    /// <returns>ProductDto mapeado</returns>
    ProductDto MapToDto(Product product);
    
    /// <summary>
    /// Mapeia uma coleção de entidades Product para ProductDto
    /// </summary>
    /// <param name="products">Coleção de entidades Product</param>
    /// <returns>Coleção de ProductDto mapeados</returns>
    IEnumerable<ProductDto> MapToDtos(IEnumerable<Product> products);
    
    /// <summary>
    /// Mapeia um CreateProductRequestDto para entidade Product
    /// </summary>
    /// <param name="request">Request de criação</param>
    /// <returns>Entidade Product</returns>
    Product MapToEntity(CreateProductRequestDto request);
    
    /// <summary>
    /// Atualiza uma entidade Product com dados do UpdateProductRequest
    /// </summary>
    /// <param name="request">Request de atualização</param>
    /// <param name="product">Entidade Product a ser atualizada</param>
    void MapToEntity(UpdateProductRequest request, Product product);
}