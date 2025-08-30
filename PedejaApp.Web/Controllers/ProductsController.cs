using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Web.Models.DTOs;
using WebDTOs = PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Web.Controllers;

/// <summary>
/// Controller para gerenciamento de produtos seguindo princípios SOLID e Clean Architecture
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    IProductService productService,
    IProductMapper productMapper,
    IProductValidationService validationService,
    ILogger<ProductsController> logger) : ControllerBase
{
    private readonly IProductService _productService = productService;
    private readonly IProductMapper _productMapper = productMapper;
    private readonly IProductValidationService _validationService = validationService;
    private readonly ILogger<ProductsController> _logger = logger;



    /// <summary>
    /// Get all products with optional filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProducts([FromQuery] ProductSearchRequest request)
    {
        try
        {
            var products = request switch
            {
                { SearchTerm: not null } => await _productService.SearchProductsAsync(request.SearchTerm),
                { MinPrice: not null, MaxPrice: not null } => await _productService.GetProductsByPriceRangeAsync(request.MinPrice.Value, request.MaxPrice.Value),
                { CategoryId: not null } => await _productService.GetProductsByCategoryAsync(request.CategoryId.Value),
                { IsActive: true } => await _productService.GetActiveProductsAsync(),
                _ => await _productService.GetAllProductsAsync()
            };

            var productDtos = _productMapper.MapToDtos(products);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse((IEnumerable<ProductDto>)productDtos, "Produtos recuperados com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos com filtros: {@Request}", request);
            return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.ErrorResponse("Erro interno do servidor"));
        }
    }

        /// <summary>
        /// Get products for public menu (only active products and categories)
        /// </summary>
        [HttpGet("menu")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProductsForMenu()
        {
            try
            {
                var products = await _productService.GetProductsForMenuAsync();
                var productDtos = _productMapper.MapToDtos(products);
                return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse((IEnumerable<ProductDto>)productDtos, "Menu recuperado com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos do menu");
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.ErrorResponse("Erro interno do servidor"));
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("by-category/{categoryId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId);
                var productDtos = _productMapper.MapToDtos(products);
                return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse((IEnumerable<ProductDto>)productDtos, "Produtos da categoria recuperados com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos da categoria {CategoryId}", categoryId);
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.ErrorResponse("Erro interno do servidor"));
            }
        }

        /// <summary>
        /// Get specific product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound(ApiResponse<ProductDto>.ErrorResponse("Produto não encontrado"));

                var productDto = _productMapper.MapToDto(product);
                return Ok(ApiResponse<ProductDto>.SuccessResponse(productDto, "Produto recuperado com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produto {ProductId}", id);
                return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse("Erro interno do servidor"));
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return BadRequest(ApiResponse<ProductDto>.ErrorResponse(errors));
            }

            try
            {
                // Validação de negócio
                var validationResult = await _validationService.ValidateCreateProductAsync(request);
                if (validationResult.Any())
                    return BadRequest(ApiResponse<ProductDto>.ErrorResponse(validationResult.ToArray()));

                var product = await _productService.CreateProductFromDtoAsync(request);
                var productDto = _productMapper.MapToDto(product);

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, 
                    ApiResponse<ProductDto>.SuccessResponse(productDto, "Produto criado com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar produto: {@Request}", request);
                return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse("Erro interno do servidor"));
            }
        }

        /// <summary>
        /// Update existing product
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return BadRequest(ApiResponse<ProductDto>.ErrorResponse(errors));
            }

            try
            {
                var updatedProduct = await _productService.UpdateProductFromDtoAsync(id, request);
                var productDto = _productMapper.MapToDto(updatedProduct);

                return Ok(ApiResponse<ProductDto>.SuccessResponse(productDto, "Produto atualizado com sucesso"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao atualizar produto {ProductId}", id);
                return BadRequest(ApiResponse<ProductDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar produto {ProductId}: {@Request}", id, request);
                return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse("Erro interno do servidor"));
            }
        }

        /// <summary>
        /// Delete product
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Produto não encontrado"));
                }

                // Validação de negócio - verificar se pode ser deletado
                var canDelete = await _validationService.CanDeleteProductAsync(id);
                if (!canDelete)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Não é possível excluir este produto pois ele possui pedidos ativos"));
                }

                await _productService.DeleteProductAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(new { }, $"Produto {id} excluído com sucesso"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao excluir produto {ProductId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir produto {ProductId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno do servidor"));
            }
        }

    /// <summary>
    /// Toggle product active status
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> ToggleProductStatus(int id)
    {
        try
        {
            var updatedProduct = await _productService.ToggleProductStatusAsync(id);
            var productDto = _productMapper.MapToDto(updatedProduct);
            
            return Ok(ApiResponse<ProductDto>.SuccessResponse(productDto, "Status do produto alterado com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar status do produto com ID {ProductId}", id);
            return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse("Erro interno do servidor"));
        }
    }
}
