using PedejaApp.Application.Common.Interfaces;

namespace PedejaApp.Application.Common.Models;

public class FileUploadResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FileName { get; set; }
    public string? OriginalFileName { get; set; }
    public string? FilePath { get; set; }
    public string? FileUrl { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public DateTime UploadDate { get; set; }
    public List<string> ImageVariants { get; set; } = new();
    public ImageMetadata? ImageMetadata { get; set; }
    public List<string> Variants { get; set; } = new();
}