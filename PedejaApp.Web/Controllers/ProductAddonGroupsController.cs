using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Web.Models.DTOs;
using PedejaApp.Domain.DTOs;
namespace PedejaApp.Web.Controllers;

/// <summary>
/// Controller para gerenciamento de grupos de adicionais de produtos seguindo princípios SOLID e Clean Architecture
/// </summary>
[ApiController]
[Route("api/products/{productId:int}/addon-groups")]
public class ProductAddonGroupsController(
    IProductAddonGroupService productAddonGroupService,
    IProductAddonGroupMapper productAddonGroupMapper,
    ILogger<ProductAddonGroupsController> logger) : ControllerBase
{
    private readonly IProductAddonGroupService _productAddonGroupService = productAddonGroupService;
    private readonly IProductAddonGroupMapper _productAddonGroupMapper = productAddonGroupMapper;
    private readonly ILogger<ProductAddonGroupsController> _logger = logger;

    /// <summary>
    /// Obter grupos de adicionais de um produto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductAddonGroupResponseDto>>>> GetProductAddonGroups(int productId)
    {
        try
        {
            var productAddonGroups = await _productAddonGroupService.GetProductAddonGroupsAsync(productId);
            var response = _productAddonGroupMapper.MapToDtos(productAddonGroups);
            
            return Ok(ApiResponse<IEnumerable<ProductAddonGroupResponseDto>>.SuccessResponse(
                response, 
                "Grupos de adicionais obtidos com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter grupos de adicionais do produto {ProductId}", productId);
            return StatusCode(500, ApiResponse.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obter produto com todos os grupos de adicionais
    /// </summary>
    [HttpGet("~/api/products/{productId:int}/with-addons")]
    public async Task<ActionResult<ApiResponse<ProductWithAddonsResponseDto>>> GetProductWithAddons(int productId)
    {
        try
        {
            var product = await _productAddonGroupService.GetProductWithAddonsAsync(productId);
            
            if (product == null)
                return NotFound(ApiResponse.ErrorResponse($"Produto com ID {productId} não encontrado"));

            return Ok(ApiResponse<ProductWithAddonsResponseDto>.SuccessResponse(
                product, 
                "Produto com adicionais obtido com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produto {ProductId} com adicionais", productId);
            return StatusCode(500, ApiResponse.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Adicionar grupo de adicionais a um produto
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductAddonGroupResponseDto>>> AddAddonGroupToProduct(
        int productId, 
        [FromBody] AddProductAddonGroupRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.ErrorResponse("Dados inválidos", ModelState));

        try
        {
            var productAddonGroup = await _productAddonGroupService.AddAddonGroupToProductAsync(productId, request);
            var response = _productAddonGroupMapper.MapToDto(productAddonGroup);

            return CreatedAtAction(
                nameof(GetProductAddonGroups), 
                new { productId }, 
                ApiResponse<ProductAddonGroupResponseDto>.SuccessResponse(
                    response, 
                    "Grupo de adicionais adicionado com sucesso"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar grupo de adicionais {AddonGroupId} ao produto {ProductId}", 
                request.AddonGroupId, productId);
            return StatusCode(500, ApiResponse.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Atualizar configuração de grupo de adicionais em um produto
    /// </summary>
    [HttpPut("{addonGroupId:int}")]
    public async Task<ActionResult<ApiResponse<ProductAddonGroupResponseDto>>> UpdateProductAddonGroup(
        int productId, 
        int addonGroupId, 
        [FromBody] UpdateProductAddonGroupRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.ErrorResponse("Dados inválidos", ModelState));

        try
        {
            var productAddonGroup = await _productAddonGroupService.UpdateProductAddonGroupAsync(
                productId, addonGroupId, request);
            
            var response = _productAddonGroupMapper.MapToDto(productAddonGroup);

            return Ok(ApiResponse<ProductAddonGroupResponseDto>.SuccessResponse(
                response, 
                "Grupo de adicionais atualizado com sucesso"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar grupo de adicionais {AddonGroupId} do produto {ProductId}", 
                addonGroupId, productId);
            return StatusCode(500, ApiResponse.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Remover grupo de adicionais de um produto
    /// </summary>
    [HttpDelete("{addonGroupId:int}")]
    public async Task<ActionResult<ApiResponse>> RemoveAddonGroupFromProduct(int productId, int addonGroupId)
    {
        try
        {
            await _productAddonGroupService.RemoveAddonGroupFromProductAsync(productId, addonGroupId);
            return Ok(ApiResponse.SuccessResponse("Grupo de adicionais removido com sucesso"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover grupo de adicionais {AddonGroupId} do produto {ProductId}", 
                addonGroupId, productId);
            return StatusCode(500, ApiResponse.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Reordenar grupos de adicionais de um produto
    /// </summary>
    [HttpPut("reorder")]
    public async Task<ActionResult<ApiResponse>> ReorderProductAddonGroups(
        int productId, 
        [FromBody] Dictionary<int, int> groupOrders)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.ErrorResponse("Dados inválidos", ModelState));

        try
        {
            await _productAddonGroupService.ReorderProductAddonGroupsAsync(productId, groupOrders);
            return Ok(ApiResponse.SuccessResponse("Grupos de adicionais reordenados com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reordenar grupos de adicionais do produto {ProductId}", productId);
            return StatusCode(500, ApiResponse.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Adicionar múltiplos grupos de adicionais a um produto
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductAddonGroupResponseDto>>>> BulkAddAddonGroupsToProduct(
        int productId, 
        [FromBody] IEnumerable<AddProductAddonGroupRequestDto> requests)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.ErrorResponse("Dados inválidos", ModelState));

        try
        {
            var productAddonGroups = await _productAddonGroupService.BulkAddAddonGroupsToProductAsync(productId, requests);
            var response = _productAddonGroupMapper.MapToDtos(productAddonGroups);

            return Ok(ApiResponse<IEnumerable<ProductAddonGroupResponseDto>>.SuccessResponse(
                response, 
                "Grupos de adicionais adicionados em lote com sucesso"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar grupos de adicionais em lote ao produto {ProductId}", productId);
            return StatusCode(500, ApiResponse.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Remover múltiplos grupos de adicionais de um produto
    /// </summary>
    [HttpDelete("bulk")]
    public async Task<ActionResult<ApiResponse>> BulkRemoveAddonGroupsFromProduct(
        int productId, 
        [FromBody] IEnumerable<int> addonGroupIds)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.ErrorResponse("Dados inválidos", ModelState));

        try
        {
            await _productAddonGroupService.BulkRemoveAddonGroupsFromProductAsync(productId, addonGroupIds);
            return Ok(ApiResponse.SuccessResponse("Grupos de adicionais removidos em lote com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover grupos de adicionais em lote do produto {ProductId}", productId);
            return StatusCode(500, ApiResponse.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Verificar se um grupo de adicionais está associado a um produto
    /// </summary>
    [HttpGet("{addonGroupId:int}/exists")]
    public async Task<ActionResult<ApiResponse<bool>>> IsAddonGroupAssignedToProduct(int productId, int addonGroupId)
    {
        try
        {
            var exists = await _productAddonGroupService.IsAddonGroupAssignedToProductAsync(productId, addonGroupId);
            return Ok(ApiResponse<bool>.SuccessResponse(
                exists, 
                "Verificação de associação realizada com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar associação do grupo {AddonGroupId} ao produto {ProductId}", 
                addonGroupId, productId);
            return StatusCode(500, ApiResponse.ErrorResponse("Erro interno do servidor"));
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
