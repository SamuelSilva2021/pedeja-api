using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;

namespace PedejaApp.Web.Controllers;

[ApiController]
[Route("api/products/{productId:int}/addon-groups")]
public class ProductAddonGroupsController : ControllerBase
{
    private readonly IProductAddonGroupService _productAddonGroupService;
    private readonly ILogger<ProductAddonGroupsController> _logger;

    public ProductAddonGroupsController(
        IProductAddonGroupService productAddonGroupService, 
        ILogger<ProductAddonGroupsController> logger)
    {
        _productAddonGroupService = productAddonGroupService;
        _logger = logger;
    }

    /// <summary>
    /// Obter grupos de adicionais de um produto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductAddonGroupResponseDto>>> GetProductAddonGroups(int productId)
    {
        try
        {
            var productAddonGroups = await _productAddonGroupService.GetProductAddonGroupsAsync(productId);
            
            var response = productAddonGroups.Select(pag => new ProductAddonGroupResponseDto
            {
                Id = pag.Id,
                ProductId = pag.ProductId,
                AddonGroupId = pag.AddonGroupId,
                AddonGroup = new AddonGroupResponseDto
                {
                    Id = pag.AddonGroup.Id,
                    Name = pag.AddonGroup.Name,
                    Description = pag.AddonGroup.Description,
                    Type = pag.AddonGroup.Type,
                    MinSelections = pag.AddonGroup.MinSelections,
                    MaxSelections = pag.AddonGroup.MaxSelections,
                    IsRequired = pag.AddonGroup.IsRequired,
                    DisplayOrder = pag.AddonGroup.DisplayOrder,
                    Addons = pag.AddonGroup.Addons.Select(a => new AddonResponseDto
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
                },
                DisplayOrder = pag.DisplayOrder,
                IsRequired = pag.IsRequired,
                MinSelectionsOverride = pag.MinSelectionsOverride,
                MaxSelectionsOverride = pag.MaxSelectionsOverride
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter grupos de adicionais do produto {ProductId}", productId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obter produto com todos os grupos de adicionais
    /// </summary>
    [HttpGet("~/api/products/{productId:int}/with-addons")]
    public async Task<ActionResult<ProductWithAddonsResponseDto>> GetProductWithAddons(int productId)
    {
        try
        {
            var product = await _productAddonGroupService.GetProductWithAddonsAsync(productId);
            
            if (product == null)
                return NotFound($"Produto com ID {productId} não encontrado");

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produto {ProductId} com adicionais", productId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Adicionar grupo de adicionais a um produto
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductAddonGroupResponseDto>> AddAddonGroupToProduct(
        int productId, 
        [FromBody] AddProductAddonGroupRequestDto request)
    {
        try
        {
            var productAddonGroup = await _productAddonGroupService.AddAddonGroupToProductAsync(productId, request);
            
            var response = new ProductAddonGroupResponseDto
            {
                Id = productAddonGroup.Id,
                ProductId = productAddonGroup.ProductId,
                AddonGroupId = productAddonGroup.AddonGroupId,
                AddonGroup = new AddonGroupResponseDto
                {
                    Id = productAddonGroup.AddonGroup.Id,
                    Name = productAddonGroup.AddonGroup.Name,
                    Description = productAddonGroup.AddonGroup.Description,
                    Type = productAddonGroup.AddonGroup.Type,
                    MinSelections = productAddonGroup.AddonGroup.MinSelections,
                    MaxSelections = productAddonGroup.AddonGroup.MaxSelections,
                    IsRequired = productAddonGroup.AddonGroup.IsRequired,
                    DisplayOrder = productAddonGroup.AddonGroup.DisplayOrder
                },
                DisplayOrder = productAddonGroup.DisplayOrder,
                IsRequired = productAddonGroup.IsRequired,
                MinSelectionsOverride = productAddonGroup.MinSelectionsOverride,
                MaxSelectionsOverride = productAddonGroup.MaxSelectionsOverride
            };

            return CreatedAtAction(nameof(GetProductAddonGroups), new { productId }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar grupo de adicionais {AddonGroupId} ao produto {ProductId}", 
                request.AddonGroupId, productId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Atualizar configuração de grupo de adicionais em um produto
    /// </summary>
    [HttpPut("{addonGroupId:int}")]
    public async Task<ActionResult<ProductAddonGroupResponseDto>> UpdateProductAddonGroup(
        int productId, 
        int addonGroupId, 
        [FromBody] UpdateProductAddonGroupRequestDto request)
    {
        try
        {
            var productAddonGroup = await _productAddonGroupService.UpdateProductAddonGroupAsync(
                productId, addonGroupId, request);
            
            var response = new ProductAddonGroupResponseDto
            {
                Id = productAddonGroup.Id,
                ProductId = productAddonGroup.ProductId,
                AddonGroupId = productAddonGroup.AddonGroupId,
                AddonGroup = new AddonGroupResponseDto
                {
                    Id = productAddonGroup.AddonGroup.Id,
                    Name = productAddonGroup.AddonGroup.Name,
                    Description = productAddonGroup.AddonGroup.Description,
                    Type = productAddonGroup.AddonGroup.Type,
                    MinSelections = productAddonGroup.AddonGroup.MinSelections,
                    MaxSelections = productAddonGroup.AddonGroup.MaxSelections,
                    IsRequired = productAddonGroup.AddonGroup.IsRequired,
                    DisplayOrder = productAddonGroup.AddonGroup.DisplayOrder
                },
                DisplayOrder = productAddonGroup.DisplayOrder,
                IsRequired = productAddonGroup.IsRequired,
                MinSelectionsOverride = productAddonGroup.MinSelectionsOverride,
                MaxSelectionsOverride = productAddonGroup.MaxSelectionsOverride
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar grupo de adicionais {AddonGroupId} do produto {ProductId}", 
                addonGroupId, productId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Remover grupo de adicionais de um produto
    /// </summary>
    [HttpDelete("{addonGroupId:int}")]
    public async Task<ActionResult> RemoveAddonGroupFromProduct(int productId, int addonGroupId)
    {
        try
        {
            await _productAddonGroupService.RemoveAddonGroupFromProductAsync(productId, addonGroupId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover grupo de adicionais {AddonGroupId} do produto {ProductId}", 
                addonGroupId, productId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Reordenar grupos de adicionais de um produto
    /// </summary>
    [HttpPut("reorder")]
    public async Task<ActionResult> ReorderProductAddonGroups(
        int productId, 
        [FromBody] Dictionary<int, int> groupOrders)
    {
        try
        {
            await _productAddonGroupService.ReorderProductAddonGroupsAsync(productId, groupOrders);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reordenar grupos de adicionais do produto {ProductId}", productId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Adicionar múltiplos grupos de adicionais a um produto
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<ProductAddonGroupResponseDto>>> BulkAddAddonGroupsToProduct(
        int productId, 
        [FromBody] IEnumerable<AddProductAddonGroupRequestDto> requests)
    {
        try
        {
            var productAddonGroups = await _productAddonGroupService.BulkAddAddonGroupsToProductAsync(productId, requests);
            
            var response = productAddonGroups.Select(pag => new ProductAddonGroupResponseDto
            {
                Id = pag.Id,
                ProductId = pag.ProductId,
                AddonGroupId = pag.AddonGroupId,
                AddonGroup = new AddonGroupResponseDto
                {
                    Id = pag.AddonGroup.Id,
                    Name = pag.AddonGroup.Name,
                    Description = pag.AddonGroup.Description,
                    Type = pag.AddonGroup.Type,
                    MinSelections = pag.AddonGroup.MinSelections,
                    MaxSelections = pag.AddonGroup.MaxSelections,
                    IsRequired = pag.AddonGroup.IsRequired,
                    DisplayOrder = pag.AddonGroup.DisplayOrder
                },
                DisplayOrder = pag.DisplayOrder,
                IsRequired = pag.IsRequired,
                MinSelectionsOverride = pag.MinSelectionsOverride,
                MaxSelectionsOverride = pag.MaxSelectionsOverride
            });

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar grupos de adicionais em lote ao produto {ProductId}", productId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Remover múltiplos grupos de adicionais de um produto
    /// </summary>
    [HttpDelete("bulk")]
    public async Task<ActionResult> BulkRemoveAddonGroupsFromProduct(
        int productId, 
        [FromBody] IEnumerable<int> addonGroupIds)
    {
        try
        {
            await _productAddonGroupService.BulkRemoveAddonGroupsFromProductAsync(productId, addonGroupIds);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover grupos de adicionais em lote do produto {ProductId}", productId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Verificar se um grupo de adicionais está associado a um produto
    /// </summary>
    [HttpGet("{addonGroupId:int}/exists")]
    public async Task<ActionResult<bool>> IsAddonGroupAssignedToProduct(int productId, int addonGroupId)
    {
        try
        {
            var exists = await _productAddonGroupService.IsAddonGroupAssignedToProductAsync(productId, addonGroupId);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar associação do grupo {AddonGroupId} ao produto {ProductId}", 
                addonGroupId, productId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}

/// <summary>
/// Controller para operações globais de produtos com adicionais
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductsWithAddonsController : ControllerBase
{
    private readonly IProductAddonGroupService _productAddonGroupService;
    private readonly ILogger<ProductsWithAddonsController> _logger;

    public ProductsWithAddonsController(
        IProductAddonGroupService productAddonGroupService, 
        ILogger<ProductsWithAddonsController> logger)
    {
        _productAddonGroupService = productAddonGroupService;
        _logger = logger;
    }

    /// <summary>
    /// Obter todos os produtos com seus grupos de adicionais
    /// </summary>
    [HttpGet("with-addons")]
    public async Task<ActionResult<IEnumerable<ProductWithAddonsResponseDto>>> GetAllProductsWithAddons()
    {
        try
        {
            var products = await _productAddonGroupService.GetAllProductsWithAddonsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todos os produtos com adicionais");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obter produtos que usam um grupo de adicionais específico
    /// </summary>
    [HttpGet("~/api/addon-groups/{addonGroupId:int}/products")]
    public async Task<ActionResult<IEnumerable<object>>> GetProductsWithAddonGroup(int addonGroupId)
    {
        try
        {
            var products = await _productAddonGroupService.GetProductsWithAddonGroupAsync(addonGroupId);
            
            var response = products.Select(p => new 
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos que usam o grupo de adicionais {AddonGroupId}", addonGroupId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}
