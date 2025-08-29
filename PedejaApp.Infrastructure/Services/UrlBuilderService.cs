using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PedejaApp.Application.Services.Interfaces;

namespace PedejaApp.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de construção de URLs
/// </summary>
public class UrlBuilderService : IUrlBuilderService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _cdnUrl;
    private readonly bool _enableCdn;

    public UrlBuilderService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _baseUrl = _configuration["FileStorage:BaseUrl"] ?? "/uploads";
        _cdnUrl = _configuration["FileStorage:CdnUrl"] ?? string.Empty;
        _enableCdn = bool.TryParse(_configuration["FileStorage:EnableCdn"], out var enableCdn) && enableCdn;
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

        // Remove leading slash if present
        var cleanPath = relativePath.TrimStart('/');
        
        // Use CDN se habilitado, senão use BaseUrl configurado
        if (_enableCdn && !string.IsNullOrWhiteSpace(_cdnUrl))
        {
            return $"{_cdnUrl.TrimEnd('/')}/uploads/{cleanPath}";
        }
        
        // Se BaseUrl é uma URL completa, use diretamente
        if (_baseUrl.StartsWith("http://") || _baseUrl.StartsWith("https://"))
        {
            return $"{_baseUrl.TrimEnd('/')}/{cleanPath}";
        }
        
        // Fallback: tentar construir URL usando HttpContext (desenvolvimento)
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
        {
            var scheme = request.Scheme;
            var host = request.Host.Value;
            return $"{scheme}://{host}{_baseUrl.TrimEnd('/')}/{cleanPath}";
        }
        
        // Último recurso: retornar URL relativa
        return $"{_baseUrl.TrimEnd('/')}/{cleanPath}";
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