using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Domain.Entities
{
    public class PaymentRefund
    {
        public int Id { get; set; }
        
        [Required]
        public int PaymentId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
        
        public DateTime RefundedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(100)]
        public string RefundedBy { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? GatewayRefundId { get; set; }
        
        [MaxLength(1000)]
        public string? GatewayResponse { get; set; }
        
        // Navigation property
        public virtual Payment Payment { get; set; } = null!;
    }
}
