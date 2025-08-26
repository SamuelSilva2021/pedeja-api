using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Domain.DTOs;
using Microsoft.Extensions.Logging;

namespace PedejaApp.Application.Services;

public class ProductAddonGroupService(IProductAddonGroupRepository productAddonGroupRepository, IProductRepository productRepository, IAddonGroupRepository addonGroupRepository, ILogger<ProductAddonGroupService> logger) : IProductAddonGroupService
{
    private readonly IProductAddonGroupRepository _productAddonGroupRepository = productAddonGroupRepository;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IAddonGroupRepository _addonGroupRepository = addonGroupRepository;
    private readonly ILogger<ProductAddonGroupService> _logger = logger;

    public async Task<IEnumerable<ProductAddonGroup>> GetProductAddonGroupsAsync(int productId)
    {
        try
        {
            _logger.LogInformation("Buscando grupos de adicionais para o produto {ProductId}", productId);
            var result = await _productAddonGroupRepository.GetByProductIdAsync(productId);
            _logger.LogInformation("Encontrados {Count} grupos de adicionais para o produto {ProductId}", result.Count(), productId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar grupos de adicionais para o produto {ProductId}", productId);
            throw;
        }
    }

    public async Task<ProductWithAddonsResponseDto?> GetProductWithAddonsAsync(int productId)
    {
        try
        {
            _logger.LogInformation("Buscando produto {ProductId} com adicionais", productId);
            
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Produto {ProductId} não encontrado", productId);
                return null;
            }

            var addonGroups = await _productAddonGroupRepository.GetByProductIdAsync(productId);
            _logger.LogInformation("Produto {ProductId} encontrado com {AddonGroupsCount} grupos de adicionais", productId, addonGroups.Count());

            return new ProductWithAddonsResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            AddonGroups = addonGroups
                .OrderBy(pag => pag.DisplayOrder)
                .Select(pag => new AddonGroupResponseDto
                {
                    Id = pag.AddonGroup.Id,
                    Name = pag.AddonGroup.Name,
                    Description = pag.AddonGroup.Description,
                    Type = pag.AddonGroup.Type,
                    MinSelections = pag.MinSelectionsOverride ?? pag.AddonGroup.MinSelections,
                    MaxSelections = pag.MaxSelectionsOverride ?? pag.AddonGroup.MaxSelections,
                    IsRequired = pag.IsRequired,
                    DisplayOrder = pag.DisplayOrder,
                    Addons = pag.AddonGroup.Addons
                        .OrderBy(a => a.DisplayOrder)
                        .Select(a => new AddonResponseDto
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Description = a.Description,
                            Price = a.Price,
                            ImageUrl = a.ImageUrl,
                            DisplayOrder = a.DisplayOrder,
                            AddonGroupId = a.AddonGroupId,
                            IsActive = a.IsActive
                        }).ToList()
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto {ProductId} com adicionais", productId);
            throw;
        }
    }

    public async Task<ProductAddonGroup> AddAddonGroupToProductAsync(int productId, AddProductAddonGroupRequestDto request)
    {
        try
        {
            _logger.LogInformation("Adicionando grupo de adicionais {AddonGroupId} ao produto {ProductId}", request.AddonGroupId, productId);
            
            // Verificar se produto existe
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Produto {ProductId} não encontrado ao tentar adicionar grupo de adicionais {AddonGroupId}", productId, request.AddonGroupId);
                throw new ArgumentException($"Produto com ID {productId} não encontrado");
            }

            // Verificar se grupo de adicionais existe
            var addonGroup = await _addonGroupRepository.GetByIdAsync(request.AddonGroupId);
            if (addonGroup == null)
            {
                _logger.LogWarning("Grupo de adicionais {AddonGroupId} não encontrado ao tentar associar ao produto {ProductId}", request.AddonGroupId, productId);
                throw new ArgumentException($"Grupo de adicionais com ID {request.AddonGroupId} não encontrado");
            }

            // Verificar se já não está associado
            var existingAssociation = await _productAddonGroupRepository.ExistsAsync(productId, request.AddonGroupId);
            if (existingAssociation)
            {
                _logger.LogWarning("Tentativa de associar grupo de adicionais {AddonGroupId} já associado ao produto {ProductId}", request.AddonGroupId, productId);
                throw new InvalidOperationException("Este grupo de adicionais já está associado ao produto");
            }

            var productAddonGroup = new ProductAddonGroup
            {
                ProductId = productId,
                AddonGroupId = request.AddonGroupId,
                DisplayOrder = request.DisplayOrder,
                IsRequired = request.IsRequired,
                MinSelectionsOverride = request.MinSelectionsOverride,
                MaxSelectionsOverride = request.MaxSelectionsOverride
            };

            var result = await _productAddonGroupRepository.AddAsync(productAddonGroup);
            _logger.LogInformation("Grupo de adicionais {AddonGroupId} adicionado com sucesso ao produto {ProductId}", request.AddonGroupId, productId);
            return result;
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Erro inesperado ao adicionar grupo de adicionais {AddonGroupId} ao produto {ProductId}", request.AddonGroupId, productId);
            throw;
        }
    }

    public async Task<ProductAddonGroup> UpdateProductAddonGroupAsync(int productId, int addonGroupId, UpdateProductAddonGroupRequestDto request)
    {
        var productAddonGroup = await _productAddonGroupRepository.GetByProductAndAddonGroupAsync(productId, addonGroupId);
        if (productAddonGroup == null)
            throw new ArgumentException("Associação produto-grupo de adicionais não encontrada");

        productAddonGroup.DisplayOrder = request.DisplayOrder;
        productAddonGroup.IsRequired = request.IsRequired;
        productAddonGroup.MinSelectionsOverride = request.MinSelectionsOverride;
        productAddonGroup.MaxSelectionsOverride = request.MaxSelectionsOverride;

        return await _productAddonGroupRepository.UpdateAsync(productAddonGroup);
    }

    public async Task RemoveAddonGroupFromProductAsync(int productId, int addonGroupId)
    {
        var productAddonGroup = await _productAddonGroupRepository.GetByProductAndAddonGroupAsync(productId, addonGroupId);
        if (productAddonGroup == null)
            throw new ArgumentException("Associação produto-grupo de adicionais não encontrada");

        await _productAddonGroupRepository.DeleteAsync(productAddonGroup);
    }

    public async Task ReorderProductAddonGroupsAsync(int productId, Dictionary<int, int> groupOrders)
    {
        var productAddonGroups = await _productAddonGroupRepository.GetByProductIdAsync(productId);

        foreach (var pag in productAddonGroups)
        {
            if (groupOrders.TryGetValue(pag.AddonGroupId, out int newOrder))
            {
                pag.DisplayOrder = newOrder;
            }
        }

        await _productAddonGroupRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProductAddonGroup>> BulkAddAddonGroupsToProductAsync(int productId, IEnumerable<AddProductAddonGroupRequestDto> requests)
    {
        // Verificar se produto existe
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new ArgumentException($"Produto com ID {productId} não encontrado");

        var addonGroupIds = requests.Select(r => r.AddonGroupId).ToList();

        // Verificar se todos os grupos existem
        var existingGroups = new List<int>();
        foreach (var id in addonGroupIds)
        {
            var group = await _addonGroupRepository.GetByIdAsync(id);
            if (group != null)
                existingGroups.Add(id);
        }

        var missingGroups = addonGroupIds.Except(existingGroups).ToList();
        if (missingGroups.Any())
            throw new ArgumentException($"Grupos de adicionais não encontrados: {string.Join(", ", missingGroups)}");

        // Verificar se já não estão associados
        var existingAssociations = new List<int>();
        foreach (var id in addonGroupIds)
        {
            if (await _productAddonGroupRepository.ExistsAsync(productId, id))
                existingAssociations.Add(id);
        }

        var newAssociations = requests.Where(r => !existingAssociations.Contains(r.AddonGroupId));

        var productAddonGroups = newAssociations.Select(request => new ProductAddonGroup
        {
            ProductId = productId,
            AddonGroupId = request.AddonGroupId,
            DisplayOrder = request.DisplayOrder,
            IsRequired = request.IsRequired,
            MinSelectionsOverride = request.MinSelectionsOverride,
            MaxSelectionsOverride = request.MaxSelectionsOverride
        }).ToList();

        return await _productAddonGroupRepository.AddRangeAsync(productAddonGroups);
    }

    public async Task BulkRemoveAddonGroupsFromProductAsync(int productId, IEnumerable<int> addonGroupIds)
    {
        var productAddonGroups = new List<ProductAddonGroup>();
        foreach (var addonGroupId in addonGroupIds)
        {
            var pag = await _productAddonGroupRepository.GetByProductAndAddonGroupAsync(productId, addonGroupId);
            if (pag != null)
                productAddonGroups.Add(pag);
        }

        await _productAddonGroupRepository.DeleteRangeAsync(productAddonGroups);
    }

    public async Task<bool> IsAddonGroupAssignedToProductAsync(int productId, int addonGroupId)
    {
        return await _productAddonGroupRepository.ExistsAsync(productId, addonGroupId);
    }

    public async Task<bool> CanRemoveAddonGroupFromProductAsync(int productId, int addonGroupId)
    {
        // Verificar se há pedidos pendentes que usam este produto com adicionais
        // Por enquanto, retorna true - pode ser implementado futuramente
        return true;
    }

    public async Task<IEnumerable<Product>> GetProductsWithAddonGroupAsync(int addonGroupId)
    {
        return await _productAddonGroupRepository.GetProductsByAddonGroupAsync(addonGroupId);
    }

    public async Task<IEnumerable<ProductWithAddonsResponseDto>> GetAllProductsWithAddonsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        var activeProducts = products.Where(p => p.IsActive);

        var result = new List<ProductWithAddonsResponseDto>();

        foreach (var product in activeProducts)
        {
            var addonGroups = await _productAddonGroupRepository.GetByProductIdAsync(product.Id);

            result.Add(new ProductWithAddonsResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                AddonGroups = addonGroups
                    .OrderBy(pag => pag.DisplayOrder)
                    .Select(pag => new AddonGroupResponseDto
                    {
                        Id = pag.AddonGroup.Id,
                        Name = pag.AddonGroup.Name,
                        Description = pag.AddonGroup.Description,
                        Type = pag.AddonGroup.Type,
                        MinSelections = pag.MinSelectionsOverride ?? pag.AddonGroup.MinSelections,
                        MaxSelections = pag.MaxSelectionsOverride ?? pag.AddonGroup.MaxSelections,
                        IsRequired = pag.IsRequired,
                        DisplayOrder = pag.DisplayOrder,
                        Addons = pag.AddonGroup.Addons
                            .Where(a => a.IsActive)
                            .OrderBy(a => a.DisplayOrder)
                            .Select(a => new AddonResponseDto
                            {
                                Id = a.Id,
                                Name = a.Name,
                                Description = a.Description,
                                Price = a.Price,
                                ImageUrl = a.ImageUrl,
                                DisplayOrder = a.DisplayOrder,
                                AddonGroupId = a.AddonGroupId,
                                IsActive = a.IsActive
                            }).ToList()
                    }).ToList()
            });
        }

        return result;
    }
}

