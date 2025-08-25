using Microsoft.AspNetCore.Http;
using PedejaApp.Application.Services.Interfaces;

namespace PedejaApp.Web.Services;

/// <summary>
/// Implementação do serviço de construção de URLs usando recursos modernos do C# 13
/// </summary>
public class UrlBuilderService(IHttpContextAccessor httpContextAccessor) : IUrlBuilderService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Constrói URL completa para imagens de produtos
    /// </summary>
    public string BuildImageUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request == null)
        {
            return string.Empty;
        }

        var request = httpContext.Request;
        var baseUrl = GetBaseUrl();
        
        // Remove leading slash if present
        var cleanPath = relativePath.TrimStart('/');
        
        return $"{baseUrl}/uploads/{cleanPath}";
    }

    /// <summary>
    /// Constrói URL base do servidor
    /// </summary>
    public string GetBaseUrl()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request == null)
            return string.Empty;

        var request = httpContext.Request;
        return $"{request.Scheme}://{request.Host.Value}";
    }
}