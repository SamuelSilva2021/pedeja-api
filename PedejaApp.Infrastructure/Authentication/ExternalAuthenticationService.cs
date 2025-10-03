using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PedejaApp.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PedejaApp.Infrastructure.Authentication;

/// <summary>
/// Serviço de autenticação externa que valida tokens JWT do saas-authentication-api
/// </summary>
public class ExternalAuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalAuthenticationService> _logger;

    public ExternalAuthenticationService(
        IConfiguration configuration,
        ILogger<ExternalAuthenticationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Get JWT settings from configuration
            var jwtSecret = _configuration["Authentication:JwtSecret"];
            var jwtIssuer = _configuration["Authentication:JwtIssuer"];
            var jwtAudience = _configuration["Authentication:JwtAudience"];

            if (string.IsNullOrEmpty(jwtSecret))
            {
                _logger.LogError("JWT Secret not configured");
                return Task.FromResult(false);
            }

            var key = Encoding.UTF8.GetBytes(jwtSecret);
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
                ValidIssuer = jwtIssuer,
                ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out _);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return Task.FromResult(false);
        }
    }

    public Task<ClaimsPrincipal> GetClaimsFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            var claims = jsonToken.Claims.ToList();
            var identity = new ClaimsIdentity(claims, "jwt");
            
            return Task.FromResult(new ClaimsPrincipal(identity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting claims from token");
            return Task.FromResult(new ClaimsPrincipal());
        }
    }

    public Task<string> RefreshTokenAsync(string refreshToken)
    {
        // Refresh token deve ser implementado via chamada para o saas-authentication-api
        // Por enquanto, retorna uma exceção indicando que não está implementado
        throw new NotImplementedException("Refresh token deve ser realizado através do saas-authentication-api");
    }
}
