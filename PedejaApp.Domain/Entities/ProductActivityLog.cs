using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedejaApp.Domain.Entities
{
    public class ProductActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // 'price_change', 'toggle_availability', 'bulk_update'

        [MaxLength(200)]
        public string? PreviousValue { get; set; }

        [MaxLength(200)]
        public string? NewValue { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? UserId { get; set; }

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
