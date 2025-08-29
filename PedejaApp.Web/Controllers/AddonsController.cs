using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Web.Models.DTOs;
using WebDTOs = PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Web.Controllers;

/// <summary>
/// Controller para gerenciamento de adicionais seguindo princípios SOLID e Clean Architecture
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AddonsController(
    IAddonService addonService,
    ILogger<AddonsController> logger) : ControllerBase
{
    private readonly IAddonService _addonService = addonService;
    private readonly ILogger<AddonsController> _logger = logger;

    /// <summary>
    /// Obter todos os adicionais
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AddonResponseDto>>>> GetAddons([FromQuery] bool activeOnly = false)
    {
        try
        {
            var addons = activeOnly 
                ? await _addonService.GetActiveAddonsAsync()
                : await _addonService.GetAllAddonsAsync();

            var addonDtos = addons.Select(a => new AddonResponseDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                Price = a.Price,
                ImageUrl = a.ImageUrl,
                DisplayOrder = a.DisplayOrder,
                AddonGroupId = a.AddonGroupId,
                IsActive = a.IsActive
            });

            return Ok(ApiResponse<IEnumerable<AddonResponseDto>>.SuccessResponse(addonDtos, "Adicionais recuperados com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter adicionais");
            return StatusCode(500, ApiResponse<IEnumerable<AddonResponseDto>>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obter adicional por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AddonResponseDto>>> GetAddon(int id)
    {
        try
        {
            var addon = await _addonService.GetAddonByIdAsync(id);
            if (addon == null)
                return NotFound(ApiResponse<AddonResponseDto>.ErrorResponse("Adicional não encontrado"));

            var addonDto = new AddonResponseDto
            {
                Id = addon.Id,
                Name = addon.Name,
                Description = addon.Description,
                Price = addon.Price,
                ImageUrl = addon.ImageUrl,
                DisplayOrder = addon.DisplayOrder,
                AddonGroupId = addon.AddonGroupId,
                IsActive = addon.IsActive
            };

            return Ok(ApiResponse<AddonResponseDto>.SuccessResponse(addonDto, "Adicional recuperado com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter adicional {AddonId}", id);
            return StatusCode(500, ApiResponse<AddonResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Criar novo adicional
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AddonResponseDto>>> CreateAddon([FromBody] CreateAddonRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToArray();
            return BadRequest(ApiResponse<AddonResponseDto>.ErrorResponse(errors));
        }

        try
        {
            var addon = await _addonService.CreateAddonAsync(request);

            var addonDto = new AddonResponseDto
            {
                Id = addon.Id,
                Name = addon.Name,
                Description = addon.Description,
                Price = addon.Price,
                ImageUrl = addon.ImageUrl,
                DisplayOrder = addon.DisplayOrder,
                AddonGroupId = addon.AddonGroupId,
                IsActive = addon.IsActive
            };

            return CreatedAtAction(nameof(GetAddon), new { id = addon.Id }, 
                ApiResponse<AddonResponseDto>.SuccessResponse(addonDto, "Adicional criado com sucesso"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar adicional");
            return BadRequest(ApiResponse<AddonResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar adicional: {@Request}", request);
            return StatusCode(500, ApiResponse<AddonResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Atualizar adicional existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AddonResponseDto>>> UpdateAddon(int id, [FromBody] UpdateAddonRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToArray();
            return BadRequest(ApiResponse<AddonResponseDto>.ErrorResponse(errors));
        }

        try
        {
            var addon = await _addonService.UpdateAddonAsync(id, request);

            var addonDto = new AddonResponseDto
            {
                Id = addon.Id,
                Name = addon.Name,
                Description = addon.Description,
                Price = addon.Price,
                ImageUrl = addon.ImageUrl,
                DisplayOrder = addon.DisplayOrder,
                AddonGroupId = addon.AddonGroupId,
                IsActive = addon.IsActive
            };

            return Ok(ApiResponse<AddonResponseDto>.SuccessResponse(addonDto, "Adicional atualizado com sucesso"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar adicional {AddonId}", id);
            return BadRequest(ApiResponse<AddonResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar adicional {AddonId}: {@Request}", id, request);
            return StatusCode(500, ApiResponse<AddonResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Excluir adicional
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAddon(int id)
    {
        try
        {
            await _addonService.DeleteAddonAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(new { }, $"Adicional {id} excluído com sucesso"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Adicional não encontrado {AddonId}", id);
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao excluir adicional {AddonId}", id);
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir adicional {AddonId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Alternar status do adicional
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<AddonResponseDto>>> ToggleAddonStatus(int id)
    {
        try
        {
            var addon = await _addonService.ToggleAddonStatusAsync(id);

            var addonDto = new AddonResponseDto
            {
                Id = addon.Id,
                Name = addon.Name,
                Description = addon.Description,
                Price = addon.Price,
                ImageUrl = addon.ImageUrl,
                DisplayOrder = addon.DisplayOrder,
                AddonGroupId = addon.AddonGroupId,
                IsActive = addon.IsActive
            };

            var statusMessage = addon.IsActive ? "ativado" : "desativado";
            return Ok(ApiResponse<AddonResponseDto>.SuccessResponse(addonDto, $"Status do adicional {statusMessage} com sucesso"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Adicional não encontrado {AddonId}", id);
            return NotFound(ApiResponse<AddonResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alternar status do adicional {AddonId}", id);
            return StatusCode(500, ApiResponse<AddonResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }
}