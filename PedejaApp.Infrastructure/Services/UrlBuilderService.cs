using Microsoft.AspNetCore.Http;
using PedejaApp.Application.Services.Interfaces;

namespace PedejaApp.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de construção de URLs
/// </summary>
public class UrlBuilderService : IUrlBuilderService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UrlBuilderService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Constrói URL completa para imagens de produtos
    /// </summary>
    /// <param name="relativePath">Caminho relativo da imagem</param>
    /// <returns>URL completa da imagem ou string vazia se o caminho for inválido</returns>
    public string BuildImageUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
        {
            return relativePath;
        }

        var scheme = request.Scheme;
        var host = request.Host.Value;
        
        // Remove leading slash if present
        var cleanPath = relativePath.TrimStart('/');
        
        return $"{scheme}://{host}/uploads/{cleanPath}";
    }
    
    /// <summary>
    /// Constrói URL base do servidor
    /// </summary>
    /// <returns>URL base do servidor</returns>
    public string GetBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
        {
            return string.Empty;
        }

        var scheme = request.Scheme;
        var host = request.Host.Value;
        
        return $"{scheme}://{host}";
    }
}