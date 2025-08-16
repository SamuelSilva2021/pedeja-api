using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using PedejaApp.Application.Common.Interfaces;

namespace PedejaApp.Infrastructure.Services;

public class ImageProcessingService : IImageProcessingService
{
    private readonly IConfiguration _configuration;
    private readonly string _uploadPath;

    public ImageProcessingService(IConfiguration configuration)
    {
        _configuration = configuration;
        _uploadPath = _configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads";
    }

    public async Task<List<string>> GenerateImageVariantsAsync(string originalFilePath, string fileName)
    {
        var variants = new List<string>();
        var baseFileName = Path.GetFileNameWithoutExtension(fileName);
        var directory = Path.GetDirectoryName(originalFilePath);

        // Definir tamanhos para diferentes variantes
        var sizes = new Dictionary<string, int>
        {
            { "thumbnail", 150 },
            { "medium", 400 },
            { "large", 800 }
        };

        try
        {
            using var originalImage = await Image.LoadAsync(originalFilePath);

            foreach (var size in sizes)
            {
                var variantFileName = $"{baseFileName}_{size.Key}.webp";
                var variantPath = Path.Combine(directory!, variantFileName);

                // Criar uma cópia da imagem para redimensionamento
                using var resizedImage = originalImage.Clone(context => 
                    context.Resize(new ResizeOptions
                    {
                        Size = new Size(size.Value, size.Value),
                        Mode = ResizeMode.Max, // Manter proporção
                        Sampler = KnownResamplers.Lanczos3 // Alta qualidade
                    })
                );

                // Salvar como WebP para melhor compressão
                await resizedImage.SaveAsWebpAsync(variantPath, new WebpEncoder
                {
                    Quality = size.Key == "thumbnail" ? 75 : 85
                });

                variants.Add(variantPath);
            }

            // Converter original para WebP se não for
            var originalExt = Path.GetExtension(originalFilePath).ToLower();
            if (originalExt != ".webp")
            {
                var webpPath = Path.Combine(directory!, $"{baseFileName}_original.webp");
                
                using var webpImage = originalImage.Clone(context => { });
                await webpImage.SaveAsWebpAsync(webpPath, new WebpEncoder { Quality = 90 });
                variants.Add(webpPath);
            }

            return variants;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao processar imagem: {ex.Message}", ex);
        }
    }

    public async Task<ImageMetadata> ExtractImageMetadataAsync(string filePath)
    {
        try
        {
            using var image = await Image.LoadAsync(filePath);
            var fileInfo = new FileInfo(filePath);

            return new ImageMetadata
            {
                Width = image.Width,
                Height = image.Height,
                Format = image.Metadata.DecodedImageFormat?.Name ?? "Unknown",
                FileSize = fileInfo.Length,
                AspectRatio = (double)image.Width / image.Height,
                ColorDepth = image.PixelType.BitsPerPixel,
                HasTransparency = image.PixelType.AlphaRepresentation != 0
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao extrair metadados da imagem: {ex.Message}", ex);
        }
    }

    public async Task<bool> OptimizeImageAsync(string filePath, int quality = 85)
    {
        try
        {
            var tempPath = $"{filePath}.temp";
            var extension = Path.GetExtension(filePath).ToLower();

            using (var image = await Image.LoadAsync(filePath))
            {
                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        await image.SaveAsJpegAsync(tempPath, new JpegEncoder { Quality = quality });
                        break;
                    case ".png":
                        await image.SaveAsPngAsync(tempPath, new PngEncoder 
                        { 
                            CompressionLevel = PngCompressionLevel.BestCompression 
                        });
                        break;
                    case ".webp":
                        await image.SaveAsWebpAsync(tempPath, new WebpEncoder { Quality = quality });
                        break;
                    default:
                        return false;
                }
            }

            // Substituir arquivo original pelo otimizado
            File.Delete(filePath);
            File.Move(tempPath, filePath);

            return true;
        }
        catch (Exception ex)
        {
            // Limpar arquivo temporário se existir
            if (File.Exists($"{filePath}.temp"))
            {
                File.Delete($"{filePath}.temp");
            }
            
            throw new InvalidOperationException($"Erro ao otimizar imagem: {ex.Message}", ex);
        }
    }

    public bool IsValidImageFormat(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        
        return validExtensions.Contains(extension);
    }

    public async Task<bool> ValidateImageIntegrityAsync(string filePath)
    {
        try
        {
            using var image = await Image.LoadAsync(filePath);
            
            // Verificações básicas de integridade
            if (image.Width <= 0 || image.Height <= 0)
                return false;

            // Verificar se não é muito pequeno ou muito grande
            var minDimension = 10;
            var maxDimension = 4000;
            
            if (image.Width < minDimension || image.Height < minDimension ||
                image.Width > maxDimension || image.Height > maxDimension)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
