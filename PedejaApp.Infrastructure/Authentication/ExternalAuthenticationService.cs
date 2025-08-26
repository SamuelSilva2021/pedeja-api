using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PedejaApp.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Json;

namespace PedejaApp.Infrastructure.Authentication;

public class ExternalAuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalAuthenticationService> _logger;
    private readonly HttpClient _httpClient;

    public ExternalAuthenticationService(
        IConfiguration configuration,
        ILogger<ExternalAuthenticationService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> ValidateTokenAsync(string token)
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
                return false;
            }

            var key = Encoding.ASCII.GetBytes(jwtSecret);
            
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

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            return validatedToken != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT token");
            return false;
        }
    }

    public async Task<ClaimsPrincipal> GetClaimsFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            var claims = jsonToken.Claims.ToList();
            var identity = new ClaimsIdentity(claims, "jwt");
            
            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting claims from token");
            return new ClaimsPrincipal();
        }
    }

    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Call external authentication API to refresh token
            var externalAuthUrl = _configuration["Authentication:ExternalApiUrl"];
            
            if (string.IsNullOrEmpty(externalAuthUrl))
            {
                _logger.LogError("External authentication URL not configured");
                return string.Empty;
            }

            var request = new
            {
                refresh_token = refreshToken
            };

            var response = await _httpClient.PostAsJsonAsync($"{externalAuthUrl}/refresh", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                return result?.access_token?.ToString() ?? string.Empty;
            }

            _logger.LogWarning("Failed to refresh token. Status: {StatusCode}", response.StatusCode);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return string.Empty;
        }
    }
}
