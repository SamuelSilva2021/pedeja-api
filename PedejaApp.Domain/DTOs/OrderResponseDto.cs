using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO de resposta para pedidos
    /// </summary>
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDelivery { get; set; }
        public string? Notes { get; set; }
        public int? EstimatedPreparationMinutes { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public int? QueuePosition { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();
    }
}
