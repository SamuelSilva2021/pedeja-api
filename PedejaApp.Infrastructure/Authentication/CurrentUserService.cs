using Microsoft.AspNetCore.Http;
using PedejaApp.Domain.Interfaces;
using System.Security.Claims;

namespace PedejaApp.Infrastructure.Authentication;

/// <summary>
/// Serviço para acesso ao contexto do usuário atual autenticado
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Construtor do serviço de usuário atual
    /// </summary>
    /// <param name="httpContextAccessor">Acessor do contexto HTTP</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// ID do usuário atual
    /// </summary>
    public string UserId => GetClaimValue(ClaimTypes.NameIdentifier) ?? GetClaimValue("user_id") ?? string.Empty;

    /// <summary>
    /// Nome de usuário atual
    /// </summary>
    public string UserName => GetClaimValue(ClaimTypes.Name) ?? GetClaimValue("username") ?? string.Empty;

    /// <summary>
    /// Email do usuário atual
    /// </summary>
    public string Email => GetClaimValue(ClaimTypes.Email) ?? GetClaimValue("email") ?? string.Empty;

    /// <summary>
    /// Indica se o usuário está autenticado
    /// </summary>
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Roles do usuário atual
    /// </summary>
    public IEnumerable<string> Roles => GetClaimValues(ClaimTypes.Role);

    /// <summary>
    /// ID do tenant do usuário atual
    /// </summary>
    public string TenantId => GetClaimValue("tenant_id") ?? string.Empty;

    /// <summary>
    /// Slug do tenant do usuário atual
    /// </summary>
    public string TenantSlug => GetClaimValue("tenant_slug") ?? string.Empty;

    /// <summary>
    /// Permissões do usuário atual
    /// </summary>
    public IEnumerable<string> Permissions => GetClaimValues("permission");

    /// <summary>
    /// Grupos de acesso do usuário atual
    /// </summary>
    public IEnumerable<string> AccessGroups => GetClaimValues("access_group");

    /// <summary>
    /// Obtém o valor de uma claim específica
    /// </summary>
    /// <param name="claimType">Tipo da claim</param>
    /// <returns>Valor da claim ou null se não encontrada</returns>
    private string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Obtém todos os valores de um tipo de claim específica
    /// </summary>
    /// <param name="claimType">Tipo da claim</param>
    /// <returns>Lista de valores da claim</returns>
    private IEnumerable<string> GetClaimValues(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindAll(claimType)?.Select(c => c.Value) ?? Enumerable.Empty<string>();
    }
}