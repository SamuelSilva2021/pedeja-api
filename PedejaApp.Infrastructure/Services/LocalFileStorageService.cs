using PedejaApp.Application.Common.Interfaces;
using PedejaApp.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PedejaApp.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly string _uploadPath;
    private readonly string _baseUrl;

    public LocalFileStorageService(
        ILogger<LocalFileStorageService> logger,
        IImageProcessingService imageProcessingService,
        IConfiguration configuration)
    {
        _logger = logger;
        _imageProcessingService = imageProcessingService;
        _uploadPath = configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads";
        _baseUrl = configuration["FileStorage:BaseUrl"] ?? "/uploads";

        // Ensure upload directory exists
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<FileUploadResult> UploadImageAsync(IFormFile file, string folder)
    {
        try
        {
            // Validate file
            var validationResult = ValidateImageFile(file);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            // Create folder structure: uploads/products/2025/08/
            var currentDate = DateTime.UtcNow;
            var folderPath = Path.Combine(_uploadPath, folder, currentDate.Year.ToString(), 
                                         currentDate.Month.ToString("00"));
            
            Directory.CreateDirectory(folderPath);

            // Full file path
            var filePath = Path.Combine(folderPath, uniqueFileName);
            var relativePath = Path.GetRelativePath(_uploadPath, filePath).Replace('\\', '/');

            // Save original file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Validate image integrity
            if (!await _imageProcessingService.ValidateImageIntegrityAsync(filePath))
            {
                File.Delete(filePath);
                return new FileUploadResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Arquivo de imagem corrompido ou inválido."
                };
            }

            // Extract metadata
            var metadata = await _imageProcessingService.ExtractImageMetadataAsync(filePath);

            // Optimize original image
            await _imageProcessingService.OptimizeImageAsync(filePath, 90);

            // Generate variants (thumbnail, medium, large)
            var variants = await _imageProcessingService.GenerateImageVariantsAsync(filePath, uniqueFileName);

            // Generate file URL
            var fileUrl = $"{_baseUrl}/{relativePath}";

            _logger.LogInformation("File uploaded and processed successfully: {FilePath}, Variants: {VariantCount}", 
                filePath, variants.Count);

            return new FileUploadResult
            {
                IsSuccess = true,
                FileName = uniqueFileName,
                OriginalFileName = file.FileName,
                FilePath = relativePath,
                FileUrl = fileUrl,
                FileSize = file.Length,
                MimeType = file.ContentType,
                UploadDate = DateTime.UtcNow,
                ImageMetadata = metadata,
                Variants = variants.Select(v => Path.GetRelativePath(_uploadPath, v).Replace('\\', '/')).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
            
            return new FileUploadResult
            {
                IsSuccess = false,
                ErrorMessage = "Erro interno durante o upload do arquivo."
            };
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_uploadPath, filePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully: {FilePath}", fullPath);
                return true;
            }

            _logger.LogWarning("File not found for deletion: {FilePath}", fullPath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }

    public string GetFileUrl(string filePath)
    {
        return $"{_baseUrl}/{filePath}";
    }

    public async Task<List<string>> GetImageVariantsAsync(string originalPath)
    {
        var variants = new List<string> { originalPath };

        var directory = Path.GetDirectoryName(originalPath);
        var fileName = Path.GetFileNameWithoutExtension(originalPath);
        var extension = Path.GetExtension(originalPath);

        // Check for generated variants
        var sizes = new[] { "thumb", "medium", "large" };
        
        foreach (var size in sizes)
        {
            var variantName = $"{fileName}_{size}{extension}";
            var variantPath = Path.Combine(directory ?? "", variantName);
            var fullVariantPath = Path.Combine(_uploadPath, variantPath);

            if (File.Exists(fullVariantPath))
            {
                variants.Add(variantPath.Replace('\\', '/'));
            }
        }

        return variants;
    }

    /// <summary>
    /// Valida se o arquivo é seguro para upload
    /// </summary>
    /// <param name="file">Arquivo a ser validado</param>
    /// <returns>Resultado da validação</returns>
    private FileUploadResult ValidateImageFile(IFormFile file)
    {
        // Check if file exists
        if (file == null || file.Length == 0)
        {
            return new FileUploadResult
            {
                IsSuccess = false,
                ErrorMessage = "Arquivo não fornecido ou está vazio."
            };
        }

        // Validar nome do arquivo para evitar path traversal
        var fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            return new FileUploadResult
            {
                IsSuccess = false,
                ErrorMessage = "Nome do arquivo inválido."
            };
        }

        // Check file size (5MB limit)
        const long maxFileSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxFileSize)
        {
            return new FileUploadResult
            {
                IsSuccess = false,
                ErrorMessage = "Arquivo muito grande. Tamanho máximo permitido: 5MB."
            };
        }

        // Check file extension
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            return new FileUploadResult
            {
                IsSuccess = false,
                ErrorMessage = "Formato de arquivo não suportado. Use JPG, PNG ou WebP."
            };
        }

        // Basic MIME type validation
        var allowedMimeTypes = new[] 
        { 
            "image/jpeg", 
            "image/jpg", 
            "image/png", 
            "image/webp" 
        };
        
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return new FileUploadResult
            {
                IsSuccess = false,
                ErrorMessage = "Tipo de arquivo inválido."
            };
        }

        // Validar assinatura do arquivo (magic bytes)
        if (!ValidateFileSignature(file))
        {
            return new FileUploadResult
            {
                IsSuccess = false,
                ErrorMessage = "Arquivo não é uma imagem válida ou foi corrompido."
            };
        }

        return new FileUploadResult { IsSuccess = true };
    }

    /// <summary>
    /// Valida a assinatura do arquivo através dos magic bytes
    /// </summary>
    /// <param name="file">Arquivo a ser validado</param>
    /// <returns>True se a assinatura for válida</returns>
    private bool ValidateFileSignature(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            stream.Position = 0;

            // JPEG: FF D8 FF
            if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                return true;

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47 &&
                buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A)
                return true;

            // WebP: RIFF....WEBP
            if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46)
            {
                var webpBuffer = new byte[12];
                stream.Position = 0;
                stream.Read(webpBuffer, 0, 12);
                if (webpBuffer[8] == 0x57 && webpBuffer[9] == 0x45 && webpBuffer[10] == 0x42 && webpBuffer[11] == 0x50)
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
