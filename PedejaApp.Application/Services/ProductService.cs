using AutoMapper;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Application.Extensions;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace PedejaApp.Application.Services;

/// <summary>
/// Serviço de produtos refatorado com AutoMapper e recursos modernos do C# 13
/// </summary>
public class ProductService(
    IProductRepository productRepository, 
    ICategoryRepository categoryRepository, 
    IProductAddonGroupService productAddonGroupService,
    IMapper mapper) : IProductService
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IProductAddonGroupService _productAddonGroupService = productAddonGroupService;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<Product>> GetAllProductsAsync() => await _productRepository.GetAllAsync();

    public async Task<IEnumerable<Product>> GetActiveProductsAsync() => await _productRepository.GetActiveProductsAsync();

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId) => await _productRepository.GetProductsByCategoryAsync(categoryId);

    public async Task<Product?> GetProductByIdAsync(int id) => await _productRepository.GetByIdAsync(id);

    public async Task<Product> CreateProductAsync(Product product)
    {
        // Validate category exists and is active
        var category = await _categoryRepository.GetByIdAsync(product.CategoryId) ?? throw new ArgumentException("Category not found");
        if (!category.IsActive)
            throw new ArgumentException("Cannot add products to inactive category");

        // Validate required fields
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Product name is required");

        if (product.Price <= 0)
            throw new ArgumentException("Product price must be greater than zero");

        // Validate name uniqueness using search
        var existingProducts = await _productRepository.SearchProductsAsync(product.Name);
        if (existingProducts.Any(p => string.Equals(p.Name, product.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Product with this name already exists");

        // Set display order if not specified
        if (product.DisplayOrder == 0)
            product.DisplayOrder = await _productRepository.GetNextDisplayOrderAsync(product.CategoryId);

        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        return await _productRepository.AddAsync(product);
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        // Validate product exists
        var existingProduct = await _productRepository.GetByIdAsync(product.Id) ?? throw new ArgumentException("Product not found");

        // Validate category exists and is active
        var category = await _categoryRepository.GetByIdAsync(product.CategoryId) ?? throw new ArgumentException("Category not found");

        // Validate required fields
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Product name is required");

        if (product.Price <= 0)
            throw new ArgumentException("Product price must be greater than zero");

        // Validate name uniqueness if changed
        if (!string.Equals(product.Name, existingProduct.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existingProducts = await _productRepository.SearchProductsAsync(product.Name);
            var duplicateProduct = existingProducts.FirstOrDefault(p => 
                string.Equals(p.Name, product.Name, StringComparison.OrdinalIgnoreCase) && p.Id != product.Id);
            
            if (duplicateProduct != null)
                throw new InvalidOperationException("Product with this name already exists");
        }

        await _productRepository.UpdateAsync(product);
        return product;
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id) ?? throw new ArgumentException("Product not found");
        await _productRepository.DeleteAsync(product);
    }

    public async Task<IEnumerable<Product>> GetProductsForMenuAsync() => await _productRepository.GetProductsForMenuAsync();

    public async Task ReorderProductsAsync(Dictionary<int, int> productOrders)
    {
        // Validate all products exist
        foreach (var productId in productOrders.Keys)
        {
            if (!await _productRepository.ExistsAsync(productId))
                throw new ArgumentException($"Product with ID {productId} not found");
        }
        await _productRepository.ReorderProductsAsync(productOrders);
    }

    public async Task<Product> ToggleProductStatusAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id) ?? throw new ArgumentException("Produto não encontrado");            
        product.IsActive = !product.IsActive;
        await _productRepository.UpdateAsync(product);
        return product;
    }

    public async Task<Product> UpdateProductPriceAsync(int id, decimal newPrice)
    {
        var product = await _productRepository.GetByIdAsync(id) ?? throw new ArgumentException("Product not found");

        if (newPrice <= 0)
            throw new ArgumentException("Price must be greater than zero");

        product.Price = newPrice;
        await _productRepository.UpdateAsync(product);
        return product;
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm) =>
        await _productRepository.SearchProductsAsync(searchTerm);

    public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
            throw new ArgumentException("Invalid price range");

        return await _productRepository.GetProductsByPriceRangeAsync(minPrice, maxPrice);
    }

    public async Task<bool> CanDeleteCategoryAsync(int categoryId) => 
        !await _productRepository.HasProductsInCategoryAsync(categoryId);

    /// <summary>
    /// Cria um produto usando AutoMapper para mapeamento elegante
    /// </summary>
    /// <param name="request">Dados de criação do produto</param>
    /// <returns>Produto criado</returns>
    /// <exception cref="ArgumentNullException">Request é nulo</exception>
    /// <exception cref="ArgumentException">Categoria não encontrada ou inativa</exception>
    public async Task<Product> CreateProductFromDtoAsync(CreateProductRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        // Validar se a categoria existe e está ativa
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId)
            ?? throw new ArgumentException($"Category with ID {request.CategoryId} not found");
        
        if (!category.IsActive)
            throw new ArgumentException("Cannot create product in inactive category");

        // Usar AutoMapper para mapear DTO para entidade
        var product = _mapper.Map<Product>(request);
        
        // Criar o produto primeiro
        var createdProduct = await CreateProductAsync(product);
        
        // Se há grupos de adicionais para associar, processar eles
        if (request.AddonGroups?.Any() == true)
        {
            try
            {
                await _productAddonGroupService.BulkAddAddonGroupsToProductAsync(
                    createdProduct.Id, 
                    request.AddonGroups);
            }
            catch (Exception ex)
            {
                // Se falhar ao adicionar grupos, reverter a criação do produto
                await _productRepository.DeleteAsync(createdProduct);
                throw new InvalidOperationException($"Failed to associate addon groups: {ex.Message}", ex);
            }
        }
        
        return createdProduct;
    }

    /// <summary>
    /// Atualiza um produto usando AutoMapper para mapeamento elegante
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <param name="request">Dados de atualização</param>
    /// <returns>Produto atualizado</returns>
    /// <exception cref="ArgumentException">Produto não encontrado</exception>
    public async Task<Product> UpdateProductFromDtoAsync(int id, UpdateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var existingProduct = await _productRepository.GetByIdAsync(id)
            ?? throw new ArgumentException($"Product with ID {id} not found");

        // Validar categoria se fornecida
        if (request.CategoryId > 0)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category?.IsActive != true)
                throw new ArgumentException($"Category with ID {request.CategoryId} is invalid or inactive");
        }

        var updatedProduct = existingProduct
            .UpdateBasicInfo(request.Name, request.Description, request.Price)
            .UpdateCategory(request.CategoryId)
            .UpdateImage(request.ImageUrl)
            .UpdateStatus(request.IsActive)
            .UpdateDisplayOrder(request.DisplayOrder);

        updatedProduct.IsValid();

        var result = await UpdateProductAsync(updatedProduct);
        
        if (request.AddonGroups != null)
        {
            
            try
            {
                var currentGroups = await _productAddonGroupService.GetProductAddonGroupsAsync(id);
                var currentGroupIds = currentGroups.Select(g => g.AddonGroupId).ToList();
                
                var groupsToRemove = currentGroupIds.Except(request.AddonGroups.Select(g => g.AddonGroupId));
                if (groupsToRemove.Any())
                {
                    await _productAddonGroupService.BulkRemoveAddonGroupsFromProductAsync(id, groupsToRemove);
                }
                
                var newGroups = request.AddonGroups.Where(g => !currentGroupIds.Contains(g.AddonGroupId));
                if (newGroups.Any())
                {
                    await _productAddonGroupService.BulkAddAddonGroupsToProductAsync(id, newGroups);
                }
                
                // Atualizar grupos existentes
                var groupsToUpdate = request.AddonGroups.Where(g => currentGroupIds.Contains(g.AddonGroupId));
                foreach (var groupUpdate in groupsToUpdate)
                {
                    var updateRequest = new UpdateProductAddonGroupRequestDto
                    {
                        DisplayOrder = groupUpdate.DisplayOrder,
                        IsRequired = groupUpdate.IsRequired,
                        MinSelectionsOverride = groupUpdate.MinSelectionsOverride,
                        MaxSelectionsOverride = groupUpdate.MaxSelectionsOverride
                    };
                    
                    await _productAddonGroupService.UpdateProductAddonGroupAsync(
                        id, groupUpdate.AddonGroupId, updateRequest);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update addon groups: {ex.Message}", ex);
            }
        }

        return result;
    }

    public async Task<IEnumerable<object>> GetProductPriceHistoryAsync(int productId)
    {
        await Task.CompletedTask;
        return [];
    }

    public async Task LogProductActivityAsync(int productId, string action, string? previousValue, string? newValue, string? userId = null)
    {
        await Task.CompletedTask;
    }

    public async Task<PedejaApp.Domain.DTOs.BulkOperationResult> BulkUpdateProductsAsync(PedejaApp.Domain.DTOs.BulkUpdateRequest request)
    {
        await Task.CompletedTask;
        return new PedejaApp.Domain.DTOs.BulkOperationResult { SuccessCount = 0, FailureCount = 0, Errors = new List<string>() };
    }

    public async Task UpdateProductAvailabilityAsync(int id, bool isActive)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product != null)
        {
            product.IsActive = isActive;
            await _productRepository.UpdateAsync(product);
        }
    }
}
