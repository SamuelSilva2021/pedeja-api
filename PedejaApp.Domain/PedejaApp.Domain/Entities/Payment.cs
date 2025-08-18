using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [Required]
        public PaymentMethod Method { get; set; }
        
        [Required]
        public PaymentStatus Status { get; set; }
        
        [MaxLength(200)]
        public string? GatewayTransactionId { get; set; }
        
        [MaxLength(1000)]
        public string? GatewayResponse { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ProcessedAt { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        // Navigation properties
        public virtual Order Order { get; set; } = null!;
        public virtual ICollection<PaymentRefund> Refunds { get; set; } = new List<PaymentRefund>();
    }

    public enum PaymentMethod
    {
        CreditCard = 1,
        DebitCard = 2,
        PIX = 3,
        Cash = 4
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Processing = 1,
        Approved = 2,
        Declined = 3,
        Cancelled = 4,
        Refunded = 5
    }
}
