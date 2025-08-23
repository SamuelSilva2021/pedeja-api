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
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obter apenas categorias ativas
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetActiveCategories()
    {
        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias ativas");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obter categoria por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { error = "Categoria não encontrada" });
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categoria {CategoryId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Criar nova categoria
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createDto)
    {
        try
        {
            
            if (createDto == null)
            {
                _logger.LogWarning("CreateCategoryDto is null");
                return BadRequest(new { error = "Dados da categoria não podem ser nulos" });
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _categoryService.CreateCategoryAsync(createDto);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar categoria");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar categoria");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Endpoint de teste para verificar serialização JSON
    /// </summary>
    [HttpPost("test")]
    public ActionResult TestJson([FromBody] CreateCategoryDto createDto)
    {
        _logger.LogInformation("TEST - Received CreateCategoryDto: Name='{Name}', Description='{Description}', DisplayOrder={DisplayOrder}", 
            createDto?.Name ?? "NULL", 
            createDto?.Description ?? "NULL", 
            createDto?.DisplayOrder);
            
        return Ok(new { 
            received = createDto,
            isNull = createDto == null,
            nameValue = createDto?.Name,
            descriptionValue = createDto?.Description,
            displayOrderValue = createDto?.DisplayOrder
        });
    }

    /// <summary>
    /// Atualizar categoria existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _categoryService.UpdateCategoryAsync(id, updateDto);
            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar categoria {CategoryId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar categoria {CategoryId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Excluir categoria por ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ApiResponse> DeleteCategory(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);

        if (result.IsSuccess)
            return ApiResponse.SuccessResponse($"Categoria {id} excluída com sucesso");
        else
            return ApiResponse.ErrorResponse($"Erro ao excluir categoria {id}");
    }

    /// <summary>
    /// Alternar status ativo/inativo de uma categoria
    /// </summary>
    [HttpPut("{id}/toggle-active")]
    public async Task<ActionResult<CategoryDto>> ToggleCategoryActive(int id)
    {
        try
        {
            var category = await _categoryService.ToggleCategoryActiveAsync(id);
            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao alterar status da categoria {CategoryId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar status da categoria {CategoryId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Reordenar categorias (drag and drop)
    /// </summary>
    [HttpPatch("reorder")]
    public async Task<ActionResult> ReorderCategories([FromBody] ReorderCategoriesDto reorderDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _categoryService.ReorderCategoriesAsync(reorderDto);
            return Ok(new { message = "Ordem das categorias atualizada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reordenar categorias");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verificar se categoria pode ser excluída
    /// </summary>
    [HttpGet("{id}/can-delete")]
    public async Task<ActionResult<bool>> CanDeleteCategory(int id)
    {
        try
        {
            var canDelete = await _categoryService.CanDeleteCategoryAsync(id);
            return Ok(new { canDelete });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar se categoria {CategoryId} pode ser excluída", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}
