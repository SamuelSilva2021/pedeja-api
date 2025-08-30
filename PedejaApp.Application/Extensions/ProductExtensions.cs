using PedejaApp.Domain.Entities;
using PedejaApp.Domain.DTOs;

namespace PedejaApp.Application.Extensions;

/// <summary>
/// Extension methods para operações elegantes com produtos
/// Utiliza recursos modernos do C# 13 e .NET 9
/// </summary>
public static class ProductExtensions
{
    /// <summary>
    /// Atualiza as propriedades básicas do produto de forma fluente
    /// </summary>
    /// <param name="product">Produto a ser atualizado</param>
    /// <param name="name">Nome do produto</param>
    /// <param name="description">Descrição do produto</param>
    /// <param name="price">Preço do produto</param>
    /// <returns>O próprio produto para encadeamento fluente</returns>
    public static Product UpdateBasicInfo(this Product product, string name, string? description, decimal price)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(price));

        product.Name = name;
        product.Description = description;
        product.Price = price;
        product.UpdatedAt = DateTime.UtcNow;
        
        return product;
    }

    /// <summary>
    /// Atualiza a categoria do produto
    /// </summary>
    /// <param name="product">Produto a ser atualizado</param>
    /// <param name="categoryId">ID da nova categoria</param>
    /// <returns>O próprio produto para encadeamento fluente</returns>
    public static Product UpdateCategory(this Product product, int categoryId)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        if (categoryId <= 0)
            throw new ArgumentException("Category ID must be greater than zero", nameof(categoryId));

        product.CategoryId = categoryId;
        product.UpdatedAt = DateTime.UtcNow;
        
        return product;
    }

    /// <summary>
    /// Atualiza a URL da imagem do produto
    /// </summary>
    /// <param name="product">Produto a ser atualizado</param>
    /// <param name="imageUrl">Nova URL da imagem</param>
    /// <returns>O próprio produto para encadeamento fluente</returns>
    public static Product UpdateImage(this Product product, string? imageUrl)
    {
        ArgumentNullException.ThrowIfNull(product);

        product.ImageUrl = imageUrl;
        product.UpdatedAt = DateTime.UtcNow;
        
        return product;
    }

    /// <summary>
    /// Atualiza o status de ativação do produto
    /// </summary>
    /// <param name="product">Produto a ser atualizado</param>
    /// <param name="isActive">Status de ativação</param>
    /// <returns>O próprio produto para encadeamento fluente</returns>
    public static Product UpdateStatus(this Product product, bool isActive)
    {
        ArgumentNullException.ThrowIfNull(product);

        product.IsActive = isActive;
        product.UpdatedAt = DateTime.UtcNow;
        
        return product;
    }

    /// <summary>
    /// Atualiza a ordem de exibição do produto
    /// </summary>
    /// <param name="product">Produto a ser atualizado</param>
    /// <param name="displayOrder">Nova ordem de exibição</param>
    /// <returns>O próprio produto para encadeamento fluente</returns>
    public static Product UpdateDisplayOrder(this Product product, int displayOrder)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        if (displayOrder < 0)
            throw new ArgumentException("Display order cannot be negative", nameof(displayOrder));

        product.DisplayOrder = displayOrder;
        product.UpdatedAt = DateTime.UtcNow;
        
        return product;
    }

    /// <summary>
    /// Atualiza apenas o preço do produto (operação rápida)
    /// </summary>
    /// <param name="product">Produto a ser atualizado</param>
    /// <param name="newPrice">Novo preço</param>
    /// <returns>O próprio produto para encadeamento fluente</returns>
    public static Product UpdatePrice(this Product product, decimal newPrice)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        if (newPrice <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(newPrice));

        product.Price = newPrice;
        product.UpdatedAt = DateTime.UtcNow;
        
        return product;
    }

    /// <summary>
    /// Verifica se o produto está válido para criação/atualização
    /// </summary>
    /// <param name="product">Produto a ser validado</param>
    /// <returns>True se válido, caso contrário lança exceção</returns>
    /// <exception cref="ArgumentException">Produto inválido</exception>
    public static bool IsValid(this Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return product switch
        {
            { Name: null or "" } => throw new ArgumentException("Product name is required"),
            { Name.Length: > 100 } => throw new ArgumentException("Product name cannot exceed 100 characters"),
            { Description.Length: > 1000 } => throw new ArgumentException("Product description cannot exceed 1000 characters"),
            { Price: <= 0 } => throw new ArgumentException("Product price must be greater than zero"),
            { CategoryId: <= 0 } => throw new ArgumentException("Product must have a valid category"),
            { DisplayOrder: < 0 } => throw new ArgumentException("Display order cannot be negative"),
            _ => true
        };
    }

    /// <summary>
    /// Cria uma cópia do produto com novos valores (imutabilidade)
    /// </summary>
    /// <param name="product">Produto original</param>
    /// <param name="updates">Ação para aplicar atualizações</param>
    /// <returns>Nova instância do produto com as atualizações</returns>
    public static Product WithUpdates(this Product product, Action<Product> updates)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(updates);

        var updatedProduct = new Product
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            DisplayOrder = product.DisplayOrder,
            CreatedAt = product.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            Category = product.Category
        };

        updates(updatedProduct);
        updatedProduct.IsValid(); // Validar após as atualizações
        
        return updatedProduct;
    }
}