using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PedejaApp.Infrastructure.Data;

namespace PedejaApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")] // Apenas administradores podem acessar migrações
public class MigrationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MigrationController> _logger;

    public MigrationController(
        ApplicationDbContext context,
        ILogger<MigrationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Migra os caminhos das imagens dos produtos existentes
    /// </summary>
    [HttpPost("fix-image-paths")]
    public async Task<ActionResult<object>> FixImagePaths()
    {
        try
        {
            var products = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.ImageUrl) && 
                           !p.ImageUrl.Contains('/') && 
                           p.CreatedAt >= new DateTime(2025, 8, 1))
                .ToListAsync();

            var updatedCount = 0;

            foreach (var product in products)
            {
                // Assumir que as imagens estão em products/2025/08/ baseado na data de criação
                var year = product.CreatedAt.Year;
                var month = product.CreatedAt.Month.ToString("00");
                
                var newImageUrl = $"products/{year}/{month}/{product.ImageUrl}";
                
                _logger.LogInformation("Updating product {ProductId}: {OldPath} -> {NewPath}", 
                    product.Id, product.ImageUrl, newImageUrl);
                
                product.ImageUrl = newImageUrl;
                updatedCount++;
            }

            await _context.SaveChangesAsync();

            return Ok(new 
            {
                Success = true,
                Message = $"Updated {updatedCount} products",
                UpdatedProducts = products.Select(p => new 
                {
                    p.Id,
                    p.Name,
                    p.ImageUrl
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing image paths");
            return StatusCode(500, new { Success = false, Message = "Error fixing image paths" });
        }
    }
}
