using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;

namespace PedejaApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddonGroupsController : ControllerBase
{
    private readonly IAddonGroupService _addonGroupService;
    private readonly ILogger<AddonGroupsController> _logger;

    public AddonGroupsController(IAddonGroupService addonGroupService, ILogger<AddonGroupsController> logger)
    {
        _addonGroupService = addonGroupService;
        _logger = logger;
    }

    /// <summary>
    /// Obter todos os grupos de adicionais
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddonGroupResponseDto>>> GetAddonGroups([FromQuery] bool activeOnly = false)
    {
        try
        {
            var addonGroups = activeOnly 
                ? await _addonGroupService.GetActiveAddonGroupsAsync()
                : await _addonGroupService.GetAllAddonGroupsAsync();

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

            return Ok(addonGroupDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter grupos de adicionais");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obter grupo de adicionais por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AddonGroupResponseDto>> GetAddonGroup(int id)
    {
        try
        {
            var addonGroup = await _addonGroupService.GetAddonGroupWithAddonsAsync(id);
            if (addonGroup == null)
                return NotFound("Grupo de adicionais não encontrado");

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

            return Ok(addonGroupDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter grupo de adicionais {AddonGroupId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Criar novo grupo de adicionais
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AddonGroupResponseDto>> CreateAddonGroup([FromBody] CreateAddonGroupRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
                Addons = new List<AddonResponseDto>()
            };

            return CreatedAtAction(nameof(GetAddonGroup), new { id = addonGroup.Id }, addonGroupDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar grupo de adicionais");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Atualizar grupo de adicionais existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<AddonGroupResponseDto>> UpdateAddonGroup(int id, [FromBody] UpdateAddonGroupRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

            return Ok(addonGroupDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar grupo de adicionais {AddonGroupId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Alternar status do grupo de adicionais
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult<AddonGroupResponseDto>> ToggleAddonGroupStatus(int id)
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

            return Ok(addonGroupDto);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar status do grupo de adicionais {AddonGroupId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Excluir grupo de adicionais
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAddonGroup(int id)
    {
        try
        {
            await _addonGroupService.DeleteAddonGroupAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir grupo de adicionais {AddonGroupId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}