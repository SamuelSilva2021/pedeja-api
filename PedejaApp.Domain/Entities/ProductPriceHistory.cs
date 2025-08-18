using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedejaApp.Domain.Entities
{
    public class ProductPriceHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PreviousPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal NewPrice { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? ChangedBy { get; set; }

        [MaxLength(200)]
        public string? Reason { get; set; }

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
