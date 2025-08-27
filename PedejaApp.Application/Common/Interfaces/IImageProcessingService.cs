namespace PedejaApp.Application.Common.Interfaces;

public interface IImageProcessingService
{
    /// <summary>
    /// Gera variantes da imagem em diferentes tamanhos (thumbnail, medium, large)
    /// </summary>
    Task<List<string>> GenerateImageVariantsAsync(string originalFilePath, string fileName);

    /// <summary>
    /// Extrai metadados da imagem (dimensões, formato, etc.)
    /// </summary>
    Task<ImageMetadata> ExtractImageMetadataAsync(string filePath);

    /// <summary>
    /// Otimiza a imagem reduzindo tamanho do arquivo mantendo qualidade
    /// </summary>
    Task<bool> OptimizeImageAsync(string filePath, int quality = 85);

    /// <summary>
    /// Valida se o formato da imagem é suportado
    /// </summary>
    bool IsValidImageFormat(string fileName);

    /// <summary>
    /// Verifica a integridade da imagem
    /// </summary>
    Task<bool> ValidateImageIntegrityAsync(string filePath);
}

public class ImageMetadata
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public double AspectRatio { get; set; }
    public int ColorDepth { get; set; }
    public bool HasTransparency { get; set; }
}