using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PedejaApp.Domain.Interfaces;

namespace PedejaApp.Web.Controllers;

/// <summary>
/// Controller para teste de autenticação JWT
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthTestController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthTestController> _logger;

    public AuthTestController(ICurrentUserService currentUserService, ILogger<AuthTestController> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint público para testar se a API está funcionando
    /// </summary>
    [HttpGet("public")]
    public IActionResult GetPublic()
    {
        return Ok(new
        {
            Message = "Endpoint público acessível",
            Timestamp = DateTime.UtcNow,
            IsAuthenticated = _currentUserService.IsAuthenticated
        });
    }

    /// <summary>
    /// Endpoint protegido para testar autenticação JWT
    /// </summary>
    [HttpGet("protected")]
    [Authorize]
    public IActionResult GetProtected()
    {
        _logger.LogInformation("Usuário autenticado acessou endpoint protegido: {UserId}", _currentUserService.UserId);

        return Ok(new
        {
            Message = "Acesso autorizado ao endpoint protegido",
            User = new
            {
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName,
                Email = _currentUserService.Email,
                TenantId = _currentUserService.TenantId,
                TenantSlug = _currentUserService.TenantSlug,
                Roles = _currentUserService.Roles,
                Permissions = _currentUserService.Permissions,
                AccessGroups = _currentUserService.AccessGroups
            },
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Endpoint para testar autorização baseada em roles
    /// </summary>
    [HttpGet("admin-only")]
    [Authorize(Roles = "admin")]
    public IActionResult GetAdminOnly()
    {
        return Ok(new
        {
            Message = "Acesso autorizado para administradores",
            User = new
            {
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName,
                Roles = _currentUserService.Roles
            },
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Endpoint para validar token manualmente
    /// </summary>
    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
            var isValid = await authService.ValidateTokenAsync(request.Token);
            
            if (isValid)
            {
                var claims = await authService.GetClaimsFromTokenAsync(request.Token);
                return Ok(new
                {
                    IsValid = true,
                    Claims = claims.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }

            return Ok(new { IsValid = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar token");
            return BadRequest(new { Error = "Erro ao validar token", Details = ex.Message });
        }
    }
}

/// <summary>
/// Request para validação de token
/// </summary>
public class ValidateTokenRequest
{
    public string Token { get; set; } = string.Empty;
}