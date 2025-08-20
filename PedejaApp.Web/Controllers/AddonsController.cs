using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddonsController : ControllerBase
{
    private readonly IAddonService _addonService;
    private readonly ILogger<AddonsController> _logger;

    public AddonsController(IAddonService addonService, ILogger<AddonsController> logger)
    {
        _addonService = addonService;
        _logger = logger;
    }

    /// <summary>
    /// Obter todos os adicionais
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddonResponseDto>>> GetAddons([FromQuery] bool activeOnly = false)
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
                DisplayOrder = a.DisplayOrder
            });

            return Ok(addonDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter adicionais");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obter adicional por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AddonResponseDto>> GetAddon(int id)
    {
        try
        {
            var addon = await _addonService.GetAddonByIdAsync(id);
            if (addon == null)
                return NotFound("Adicional não encontrado");

            var addonDto = new AddonResponseDto
            {
                Id = addon.Id,
                Name = addon.Name,
                Description = addon.Description,
                Price = addon.Price,
                ImageUrl = addon.ImageUrl,
                DisplayOrder = addon.DisplayOrder
            };

            return Ok(addonDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter adicional {AddonId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Criar novo adicional
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AddonResponseDto>> CreateAddon([FromBody] CreateAddonRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var addon = await _addonService.CreateAddonAsync(request);

            var addonDto = new AddonResponseDto
            {
                Id = addon.Id,
                Name = addon.Name,
                Description = addon.Description,
                Price = addon.Price,
                ImageUrl = addon.ImageUrl,
                DisplayOrder = addon.DisplayOrder
            };

            return CreatedAtAction(nameof(GetAddon), new { id = addon.Id }, addonDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar adicional");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Atualizar adicional existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<AddonResponseDto>> UpdateAddon(int id, [FromBody] UpdateAddonRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var addon = await _addonService.UpdateAddonAsync(id, request);

            var addonDto = new AddonResponseDto
            {
                Id = addon.Id,
                Name = addon.Name,
                Description = addon.Description,
                Price = addon.Price,
                ImageUrl = addon.ImageUrl,
                DisplayOrder = addon.DisplayOrder
            };

            return Ok(addonDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar adicional {AddonId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Excluir adicional
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAddon(int id)
    {
        try
        {
            await _addonService.DeleteAddonAsync(id);
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
            _logger.LogError(ex, "Erro ao excluir adicional {AddonId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Alternar status do adicional
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult<AddonResponseDto>> ToggleAddonStatus(int id)
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
                DisplayOrder = addon.DisplayOrder
            };

            return Ok(addonDto);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alternar status do adicional {AddonId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}