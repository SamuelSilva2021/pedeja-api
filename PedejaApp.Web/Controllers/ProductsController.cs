using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Entities;
using PedejaApp.Application.Models;
using WebDTOs = PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Build complete URL for product images
        /// </summary>
        private string BuildImageUrl(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            var request = HttpContext.Request;
            var scheme = request.Scheme;
            var host = request.Host.Value;
            
            // Remove leading slash if present
            var cleanPath = relativePath.TrimStart('/');
            
            return $"{scheme}://{host}/uploads/{cleanPath}";
        }

        /// <summary>
        /// Get all products with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] ProductSearchRequest request)
        {
            try
            {
                IEnumerable<Product> products;

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    products = await _productService.SearchProductsAsync(request.SearchTerm);
                }
                else if (request.MinPrice.HasValue && request.MaxPrice.HasValue)
                {
                    products = await _productService.GetProductsByPriceRangeAsync(request.MinPrice.Value, request.MaxPrice.Value);
                }
                else if (request.CategoryId.HasValue)
                {
                    products = await _productService.GetProductsByCategoryAsync(request.CategoryId.Value);
                }
                else if (request.IsActive.HasValue && request.IsActive.Value)
                {
                    products = await _productService.GetActiveProductsAsync();
                }
                else
                {
                    products = await _productService.GetAllProductsAsync();
                }

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? "",
                    ImageUrl = BuildImageUrl(p.ImageUrl),
                    IsActive = p.IsActive,
                    DisplayOrder = p.DisplayOrder,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });

                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, "Internal server error while getting products");
            }
        }

        /// <summary>
        /// Get products for public menu (only active products and categories)
        /// </summary>
        [HttpGet("menu")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsForMenu()
        {
            try
            {
                var products = await _productService.GetProductsForMenuAsync();
                
                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? "",
                    ImageUrl = BuildImageUrl(p.ImageUrl),
                    IsActive = p.IsActive,
                    DisplayOrder = p.DisplayOrder,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });

                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting menu products");
                return StatusCode(500, "Internal server error while getting menu");
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("by-category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId);
                
                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? "",
                    ImageUrl = BuildImageUrl(p.ImageUrl),
                    IsActive = p.IsActive,
                    DisplayOrder = p.DisplayOrder,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });

                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category {CategoryId}", categoryId);
                return StatusCode(500, "Internal server error while getting products by category");
            }
        }

        /// <summary>
        /// Get specific product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound($"Product with ID {id} not found");

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category?.Name ?? "",
                    ImageUrl = BuildImageUrl(product.ImageUrl),
                    IsActive = product.IsActive,
                    DisplayOrder = product.DisplayOrder,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", id);
                return StatusCode(500, "Internal server error while getting product");
            }
        }

        /// <summary>
        /// Create new product
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = new Product
                {
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    Price = request.Price,
                    CategoryId = request.CategoryId,
                    ImageUrl = request.ImageUrl?.Trim(),
                    IsActive = request.IsActive,
                    DisplayOrder = request.DisplayOrder
                };

                var createdProduct = await _productService.CreateProductAsync(product);

                var productDto = new ProductDto
                {
                    Id = createdProduct.Id,
                    Name = createdProduct.Name,
                    Description = createdProduct.Description,
                    Price = createdProduct.Price,
                    CategoryId = createdProduct.CategoryId,
                    CategoryName = createdProduct.Category?.Name ?? "",
                    ImageUrl = createdProduct.ImageUrl,
                    IsActive = createdProduct.IsActive,
                    DisplayOrder = createdProduct.DisplayOrder,
                    CreatedAt = createdProduct.CreatedAt,
                    UpdatedAt = createdProduct.UpdatedAt
                };

                return CreatedAtAction(nameof(GetProduct), new { id = productDto.Id }, productDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "Internal server error while creating product");
            }
        }

        /// <summary>
        /// Update existing product
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = new Product
                {
                    Id = id,
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    Price = request.Price,
                    CategoryId = request.CategoryId,
                    ImageUrl = request.ImageUrl?.Trim(),
                    IsActive = request.IsActive,
                    DisplayOrder = request.DisplayOrder
                };

                var updatedProduct = await _productService.UpdateProductAsync(product);

                var productDto = new ProductDto
                {
                    Id = updatedProduct.Id,
                    Name = updatedProduct.Name,
                    Description = updatedProduct.Description,
                    Price = updatedProduct.Price,
                    CategoryId = updatedProduct.CategoryId,
                    CategoryName = updatedProduct.Category?.Name ?? "",
                    ImageUrl = updatedProduct.ImageUrl,
                    IsActive = updatedProduct.IsActive,
                    DisplayOrder = updatedProduct.DisplayOrder,
                    CreatedAt = updatedProduct.CreatedAt,
                    UpdatedAt = updatedProduct.UpdatedAt
                };

                return Ok(productDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, "Internal server error while updating product");
            }
        }

        /// <summary>
        /// Delete product
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, "Internal server error while deleting product");
            }
        }

        /// <summary>
        /// Toggle product active status
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<ProductDto>> ToggleProductStatus(int id)
        {
            try
            {
                var updatedProduct = await _productService.ToggleProductStatusAsync(id);

                var productDto = new ProductDto
                {
                    Id = updatedProduct.Id,
                    Name = updatedProduct.Name,
                    Description = updatedProduct.Description,
                    Price = updatedProduct.Price,
                    CategoryId = updatedProduct.CategoryId,
                    CategoryName = updatedProduct.Category?.Name ?? "",
                    ImageUrl = updatedProduct.ImageUrl,
                    IsActive = updatedProduct.IsActive,
                    DisplayOrder = updatedProduct.DisplayOrder,
                    CreatedAt = updatedProduct.CreatedAt,
                    UpdatedAt = updatedProduct.UpdatedAt
                };

                return Ok(productDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling product status {ProductId}", id);
                return StatusCode(500, "Internal server error while toggling product status");
            }
        }

        /// <summary>
        /// Update product price quickly
        /// </summary>
        [HttpPatch("{id}/price")]
        public async Task<ActionResult<ProductDto>> UpdateProductPrice(int id, [FromBody] UpdateProductPriceRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedProduct = await _productService.UpdateProductPriceAsync(id, request.Price);

                var productDto = new ProductDto
                {
                    Id = updatedProduct.Id,
                    Name = updatedProduct.Name,
                    Description = updatedProduct.Description,
                    Price = updatedProduct.Price,
                    CategoryId = updatedProduct.CategoryId,
                    CategoryName = updatedProduct.Category?.Name ?? "",
                    ImageUrl = updatedProduct.ImageUrl,
                    IsActive = updatedProduct.IsActive,
                    DisplayOrder = updatedProduct.DisplayOrder,
                    CreatedAt = updatedProduct.CreatedAt,
                    UpdatedAt = updatedProduct.UpdatedAt
                };

                return Ok(productDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product price {ProductId}", id);
                return StatusCode(500, "Internal server error while updating product price");
            }
        }

        /// <summary>
        /// Reorder products by updating display order
        /// </summary>
        [HttpPatch("reorder")]
        public async Task<ActionResult> ReorderProducts([FromBody] ReorderProductsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _productService.ReorderProductsAsync(request.ProductOrders);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering products");
                return StatusCode(500, "Internal server error while reordering products");
            }
        }

        /// <summary>
        /// Toggle product availability quickly
        /// </summary>
        [HttpPatch("{id}/toggle-availability")]
        public async Task<ActionResult<WebDTOs.ToggleAvailabilityResponse>> ToggleProductAvailability(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound($"Product with ID {id} not found");

                // Use existing toggle method
                var updatedProduct = await _productService.ToggleProductStatusAsync(id);

                return Ok(new WebDTOs.ToggleAvailabilityResponse
                {
                    Id = id,
                    IsActive = updatedProduct.IsActive,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling product availability {ProductId}", id);
                return StatusCode(500, "Internal server error while toggling product availability");
            }
        }

        /// <summary>
        /// Story 2.3: Quick price update for daily management
        /// </summary>
        [HttpPatch("{id}/quick-price")]
        public async Task<ActionResult<WebDTOs.QuickPriceUpdateResponse>> QuickPriceUpdate(int id, [FromBody] WebDTOs.QuickPriceUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound($"Product with ID {id} not found");

                var oldPrice = product.Price;
                var updatedProduct = await _productService.UpdateProductPriceAsync(id, request.Price);

                return Ok(new WebDTOs.QuickPriceUpdateResponse
                {
                    Id = id,
                    OldPrice = oldPrice,
                    NewPrice = updatedProduct.Price,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in quick price update for product {ProductId}", id);
                return StatusCode(500, "Internal server error while updating price");
            }
        }

        /// <summary>
        /// Story 2.3: Bulk availability update for daily management
        /// </summary>
        [HttpPatch("bulk-availability")]
        public async Task<ActionResult<WebDTOs.BulkUpdateResponse>> BulkUpdateAvailability([FromBody] WebDTOs.BulkUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var results = new List<WebDTOs.ToggleAvailabilityResponse>();
                var errors = new List<string>();

                foreach (var productId in request.ProductIds)
                {
                    try
                    {
                        var product = await _productService.GetProductByIdAsync(productId);
                        if (product == null)
                        {
                            errors.Add($"Product with ID {productId} not found");
                            continue;
                        }

                        // Only toggle if current status is different from desired
                        if (product.IsActive != request.SetActive)
                        {
                            var updatedProduct = await _productService.ToggleProductStatusAsync(productId);
                            results.Add(new WebDTOs.ToggleAvailabilityResponse
                            {
                                Id = productId,
                                IsActive = updatedProduct.IsActive,
                                Timestamp = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            // Product already in desired state
                            results.Add(new WebDTOs.ToggleAvailabilityResponse
                            {
                                Id = productId,
                                IsActive = product.IsActive,
                                Timestamp = DateTime.UtcNow
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating availability for product {ProductId}", productId);
                        errors.Add($"Error updating product {productId}: {ex.Message}");
                    }
                }

                return Ok(new WebDTOs.BulkUpdateResponse
                {
                    SuccessCount = results.Count,
                    Results = results,
                    Errors = errors,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk availability update");
                return StatusCode(500, "Internal server error while bulk updating availability");
            }
        }
    }
}
