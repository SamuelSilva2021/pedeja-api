using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO para rejeitar um pedido
    /// </summary>
    public class RejectOrderRequestDto
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string? RejectedBy { get; set; }
    }
}
