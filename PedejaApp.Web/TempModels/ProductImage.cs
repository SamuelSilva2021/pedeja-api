using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string FileName { get; set; } = null!;

    public string OriginalName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string MimeType { get; set; } = null!;

    public long FileSize { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public string? AspectRatio { get; set; }

    public DateTime UploadDate { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? ThumbnailPath { get; set; }

    public string? MediumPath { get; set; }

    public string? LargePath { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
