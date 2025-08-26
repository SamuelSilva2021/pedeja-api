using System.Security.Claims;

namespace PedejaApp.Domain.Interfaces;

public interface IAuthenticationService
{
    Task<bool> ValidateTokenAsync(string token);
    Task<ClaimsPrincipal> GetClaimsFromTokenAsync(string token);
    Task<string> RefreshTokenAsync(string refreshToken);
}

public interface ICurrentUserService
{
    string UserId { get; }
    string UserName { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
}
