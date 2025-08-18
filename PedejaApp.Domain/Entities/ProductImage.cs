using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedejaApp.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string OriginalName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string MimeType { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        [MaxLength(50)]
        public string? AspectRatio { get; set; }

        [Required]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public bool IsPrimary { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        [MaxLength(500)]
        public string? ThumbnailPath { get; set; }

        [MaxLength(500)]
        public string? MediumPath { get; set; }

        [MaxLength(500)]
        public string? LargePath { get; set; }

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
