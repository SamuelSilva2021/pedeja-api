namespace PedejaApp.Application.Services.Interfaces;

/// <summary>
/// Interface para construção de URLs de recursos
/// </summary>
public interface IUrlBuilderService
{
    /// <summary>
    /// Constrói URL completa para imagens de produtos
    /// </summary>
    /// <param name="relativePath">Caminho relativo da imagem</param>
    /// <returns>URL completa da imagem ou string vazia se o caminho for inválido</returns>
    string BuildImageUrl(string? relativePath);
    
    /// <summary>
    /// Constrói URL base do servidor
    /// </summary>
    /// <returns>URL base do servidor</returns>
    string GetBaseUrl();
}