using PedejaApp.Application.Common.Models;
using Microsoft.AspNetCore.Http;

namespace PedejaApp.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Uploads an image file and returns upload result with file information
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <param name="folder">The target folder (e.g., "products")</param>
    /// <returns>Upload result with file details</returns>
    Task<FileUploadResult> UploadImageAsync(IFormFile file, string folder);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="filePath">Relative path to the file</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteFileAsync(string filePath);

    /// <summary>
    /// Gets the public URL for a file
    /// </summary>
    /// <param name="filePath">Relative path to the file</param>
    /// <returns>Public URL for the file</returns>
    string GetFileUrl(string filePath);

    /// <summary>
    /// Gets all image variants (original, thumbnail, medium, large) for a file
    /// </summary>
    /// <param name="originalPath">Path to the original image</param>
    /// <returns>List of available image variant paths</returns>
    Task<List<string>> GetImageVariantsAsync(string originalPath);
}
