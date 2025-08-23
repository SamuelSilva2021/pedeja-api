using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Common.Interfaces;
using PedejaApp.Application.Features.Categories;
using PedejaApp.Web.Extensions;
using PedejaApp.Web.Models.DTOs;
using System.Linq;

namespace PedejaApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Obter todas as categorias ordenadas por DisplayOrder
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categories, "Categorias recuperadas com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias");
            return StatusCode(500, ApiResponse<IEnumerable<CategoryDto>>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obter apenas categorias ativas
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetActiveCategories()
    {
        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categories, "Categorias ativas recuperadas com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias ativas");
            return StatusCode(500, ApiResponse<IEnumerable<CategoryDto>>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obter categoria por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(ApiResponse<CategoryDto>.ErrorResponse("Categoria não encontrada"));
            }

            return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Categoria recuperada com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categoria {CategoryId}", id);
            return StatusCode(500, ApiResponse<CategoryDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Criar nova categoria
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto createDto)
    {
        try
        {
            
            if (createDto == null)
            {
                _logger.LogWarning("CreateCategoryDto is null");
                return BadRequest(ApiResponse<CategoryDto>.ErrorResponse("Dados da categoria não podem ser nulos"));
            }
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return BadRequest(ApiResponse<CategoryDto>.ErrorResponse(errors));
            }

            var category = await _categoryService.CreateCategoryAsync(createDto);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, 
                ApiResponse<CategoryDto>.SuccessResponse(category, "Categoria criada com sucesso"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar categoria");
            return BadRequest(ApiResponse<CategoryDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar categoria");
            return StatusCode(500, ApiResponse<CategoryDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Endpoint de teste para verificar serialização JSON
    /// </summary>
    [HttpPost("test")]
    public ActionResult<ApiResponse<object>> TestJson([FromBody] CreateCategoryDto createDto)
    {
        _logger.LogInformation("TEST - Received CreateCategoryDto: Name='{Name}', Description='{Description}', DisplayOrder={DisplayOrder}", 
            createDto?.Name ?? "NULL", 
            createDto?.Description ?? "NULL", 
            createDto?.DisplayOrder);
            
        var testData = new { 
            received = createDto,
            isNull = createDto == null,
            nameValue = createDto?.Name,
            descriptionValue = createDto?.Description,
            displayOrderValue = createDto?.DisplayOrder
        };
        
        return Ok(ApiResponse<object>.SuccessResponse(testData, "Teste de serialização JSON executado com sucesso"));
    }

    /// <summary>
    /// Atualizar categoria existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return BadRequest(ApiResponse<CategoryDto>.ErrorResponse(errors));
            }

            var category = await _categoryService.UpdateCategoryAsync(id, updateDto);
            return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Categoria atualizada com sucesso"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar categoria {CategoryId}", id);
            return BadRequest(ApiResponse<CategoryDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar categoria {CategoryId}", id);
            return StatusCode(500, ApiResponse<CategoryDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Excluir categoria por ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(int id)
    {
        try
        {
            var result = await _categoryService.DeleteCategoryAsync(id);

            if (result.IsSuccess)
                return Ok(ApiResponse<object>.SuccessResponse(new { }, $"Categoria {id} excluída com sucesso"));
            else
                return BadRequest(ApiResponse<object>.ErrorResponse(result.Error ?? "Erro ao excluir categoria"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir categoria {CategoryId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Alternar status ativo/inativo de uma categoria
    /// </summary>
    [HttpPut("{id}/toggle-active")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> ToggleCategoryActive(int id)
    {
        try
        {
            var category = await _categoryService.ToggleCategoryActiveAsync(id);
            return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Status da categoria alterado com sucesso"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao alterar status da categoria {CategoryId}", id);
            return NotFound(ApiResponse<CategoryDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar status da categoria {CategoryId}", id);
            return StatusCode(500, ApiResponse<CategoryDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Reordenar categorias (drag and drop)
    /// </summary>
    [HttpPatch("reorder")]
    public async Task<ActionResult<ApiResponse<object>>> ReorderCategories([FromBody] ReorderCategoriesDto reorderDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return BadRequest(ApiResponse<object>.ErrorResponse(errors));
            }

            await _categoryService.ReorderCategoriesAsync(reorderDto);
            return Ok(ApiResponse<object>.SuccessResponse(new { }, "Ordem das categorias atualizada com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reordenar categorias");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Verificar se categoria pode ser excluída
    /// </summary>
    [HttpGet("{id}/can-delete")]
    public async Task<ActionResult<ApiResponse<object>>> CanDeleteCategory(int id)
    {
        try
        {
            var canDelete = await _categoryService.CanDeleteCategoryAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(new { canDelete }, "Verificação realizada com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar se categoria {CategoryId} pode ser excluída", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno do servidor"));
        }
    }
}
