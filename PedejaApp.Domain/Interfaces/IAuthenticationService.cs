using System.Security.Claims;

namespace PedejaApp.Domain.Interfaces;

/// <summary>
/// Interface para serviços de autenticação externos
/// </summary>
public interface IAuthenticationService
{
    Task<bool> ValidateTokenAsync(string token);
    Task<ClaimsPrincipal> GetClaimsFromTokenAsync(string token);
    Task<string> RefreshTokenAsync(string refreshToken);
}

/// <summary>
/// Interface para acesso ao contexto do usuário atual autenticado
/// </summary>
public interface ICurrentUserService
{
    string UserId { get; }
    string UserName { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    string TenantId { get; }
    string TenantSlug { get; }
    IEnumerable<string> Permissions { get; }
    IEnumerable<string> AccessGroups { get; }
}
