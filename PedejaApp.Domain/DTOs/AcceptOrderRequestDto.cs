using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO para aceitar um pedido
    /// </summary>
    public class AcceptOrderRequestDto
    {
        [Required]
        [Range(5, 300, ErrorMessage = "Tempo estimado deve estar entre 5 e 300 minutos")]
        public int EstimatedPreparationMinutes { get; set; } = 30;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string? UserId { get; set; }
    }
}