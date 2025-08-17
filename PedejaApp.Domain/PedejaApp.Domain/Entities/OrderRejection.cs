using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedejaApp.Domain.Entities;

public class OrderRejection
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    public DateTime RejectedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    public string RejectedBy { get; set; } = string.Empty;

    // Navigation property
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;
}
