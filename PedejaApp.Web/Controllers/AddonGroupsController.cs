using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Web.Controllers;

/// <summary>
/// Controller para gerenciamento de grupos de adicionais seguindo princípios SOLID e Clean Architecture
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AddonGroupsController(
    IAddonGroupService addonGroupService,
    ILogger<AddonGroupsController> logger) : ControllerBase
{
    private readonly IAddonGroupService _addonGroupService = addonGroupService;
    private readonly ILogger<AddonGroupsController> _logger = logger;

    /// <summary>
    /// Obter todos os grupos de adicionais
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AddonGroupResponseDto>>>> GetAddonGroups()
    {
        try
        {
            var addonGroups = await _addonGroupService.GetActiveAddonGroupsAsync();

            var addonGroupDtos = addonGroups.Select(ag => new AddonGroupResponseDto
            {
                Id = ag.Id,
                Name = ag.Name,
                Description = ag.Description,
                Type = ag.Type,
                MinSelections = ag.MinSelections,
                MaxSelections = ag.MaxSelections,
                IsRequired = ag.IsRequired,
                DisplayOrder = ag.DisplayOrder,
                IsActive = ag.IsActive,
                Addons = ag.Addons.Select(a => new AddonResponseDto
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
            });

            return Ok(ApiResponse<IEnumerable<AddonGroupResponseDto>>.SuccessResponse(addonGroupDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter grupos de adicionais");
            return StatusCode(500, ApiResponse<IEnumerable<AddonGroupResponseDto>>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obter grupo de adicionais por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AddonGroupResponseDto>>> GetAddonGroup(int id)
    {
        try
        {
            var addonGroup = await _addonGroupService.GetAddonGroupWithAddonsAsync(id);
            if (addonGroup == null)
                return NotFound(ApiResponse<AddonGroupResponseDto>.ErrorResponse("Grupo de adicionais não encontrado"));

            var addonGroupDto = new AddonGroupResponseDto
            {
                Id = addonGroup.Id,
                Name = addonGroup.Name,
                Description = addonGroup.Description,
                Type = addonGroup.Type,
                MinSelections = addonGroup.MinSelections,
                MaxSelections = addonGroup.MaxSelections,
                IsRequired = addonGroup.IsRequired,
                DisplayOrder = addonGroup.DisplayOrder,
                IsActive = addonGroup.IsActive,
                Addons = addonGroup.Addons.Select(a => new AddonResponseDto
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
            };

            return Ok(ApiResponse<AddonGroupResponseDto>.SuccessResponse(addonGroupDto, "Grupo de adicionais recuperado com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter grupo de adicionais {AddonGroupId}", id);
            return StatusCode(500, ApiResponse<AddonGroupResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Criar novo grupo de adicionais
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AddonGroupResponseDto>>> CreateAddonGroup([FromBody] CreateAddonGroupRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return BadRequest(ApiResponse<AddonGroupResponseDto>.ErrorResponse(errors));
            }

            var addonGroup = await _addonGroupService.CreateAddonGroupAsync(request);

            var addonGroupDto = new AddonGroupResponseDto
            {
                Id = addonGroup.Id,
                Name = addonGroup.Name,
                Description = addonGroup.Description,
                Type = addonGroup.Type,
                MinSelections = addonGroup.MinSelections,
                MaxSelections = addonGroup.MaxSelections,
                IsRequired = addonGroup.IsRequired,
                DisplayOrder = addonGroup.DisplayOrder,
                IsActive = addonGroup.IsActive,
                Addons = new List<AddonResponseDto>()
            };

            return CreatedAtAction(nameof(GetAddonGroup), new { id = addonGroup.Id }, 
                ApiResponse<AddonGroupResponseDto>.SuccessResponse(addonGroupDto, "Grupo de adicionais criado com sucesso"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar grupo de adicionais");
            return BadRequest(ApiResponse<AddonGroupResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar grupo de adicionais");
            return StatusCode(500, ApiResponse<AddonGroupResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Atualizar grupo de adicionais existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AddonGroupResponseDto>>> UpdateAddonGroup(int id, [FromBody] UpdateAddonGroupRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return BadRequest(ApiResponse<AddonGroupResponseDto>.ErrorResponse(errors));
            }

            var addonGroup = await _addonGroupService.UpdateAddonGroupAsync(id, request);

            var addonGroupDto = new AddonGroupResponseDto
            {
                Id = addonGroup.Id,
                Name = addonGroup.Name,
                Description = addonGroup.Description,
                Type = addonGroup.Type,
                MinSelections = addonGroup.MinSelections,
                MaxSelections = addonGroup.MaxSelections,
                IsRequired = addonGroup.IsRequired,
                DisplayOrder = addonGroup.DisplayOrder,
                IsActive = addonGroup.IsActive,
                Addons = addonGroup.Addons.Select(a => new AddonResponseDto
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
            };

            return Ok(ApiResponse<AddonGroupResponseDto>.SuccessResponse(addonGroupDto, "Grupo de adicionais atualizado com sucesso"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar grupo de adicionais {AddonGroupId}", id);
            return BadRequest(ApiResponse<AddonGroupResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar grupo de adicionais {AddonGroupId}", id);
            return StatusCode(500, ApiResponse<AddonGroupResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Alternar status ativo/inativo do grupo de adicionais
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<AddonGroupResponseDto>>> ToggleAddonGroupStatus(int id)
    {
        try
        {
            var addonGroup = await _addonGroupService.ToggleAddonGroupStatusAsync(id);

            var addonGroupDto = new AddonGroupResponseDto
            {
                Id = addonGroup.Id,
                Name = addonGroup.Name,
                Description = addonGroup.Description,
                Type = addonGroup.Type,
                MinSelections = addonGroup.MinSelections,
                MaxSelections = addonGroup.MaxSelections,
                IsRequired = addonGroup.IsRequired,
                DisplayOrder = addonGroup.DisplayOrder,
                IsActive = addonGroup.IsActive,
                Addons = addonGroup.Addons.Select(a => new AddonResponseDto
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
            };

            var statusMessage = addonGroup.IsActive ? "ativado" : "desativado";
            return Ok(ApiResponse<AddonGroupResponseDto>.SuccessResponse(addonGroupDto, $"Status do grupo de adicionais {statusMessage} com sucesso"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Grupo de adicionais não encontrado {AddonGroupId}", id);
            return NotFound(ApiResponse<AddonGroupResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alternar status do grupo de adicionais {AddonGroupId}", id);
            return StatusCode(500, ApiResponse<AddonGroupResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Excluir grupo de adicionais
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAddonGroup(int id)
    {
        try
        {
            await _addonGroupService.DeleteAddonGroupAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Grupo de adicionais excluído com sucesso"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Grupo de adicionais não encontrado para exclusão {AddonGroupId}", id);
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir grupo de adicionais {AddonGroupId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno do servidor"));
        }
    }
}