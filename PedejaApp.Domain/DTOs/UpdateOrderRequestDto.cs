using System.ComponentModel.DataAnnotations;
using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO para atualizar informações básicas de um pedido
    /// </summary>
    public class UpdateOrderRequestDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? CustomerName { get; set; }

        [StringLength(20, MinimumLength = 10)]
        public string? CustomerPhone { get; set; }

        [EmailAddress]
        public string? CustomerEmail { get; set; }

        [StringLength(500, MinimumLength = 10)]
        public string? DeliveryAddress { get; set; }

        public bool? IsDelivery { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public int? EstimatedPreparationMinutes { get; set; }
    }
}