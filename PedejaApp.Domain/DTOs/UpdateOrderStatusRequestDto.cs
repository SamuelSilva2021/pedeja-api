using System.ComponentModel.DataAnnotations;
using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO para atualizar o status de um pedido
    /// </summary>
    public class UpdateOrderStatusRequestDto
    {
        [Required]
        public OrderStatus Status { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string? UserId { get; set; }
    }
}