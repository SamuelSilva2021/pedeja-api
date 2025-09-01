namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO de resposta para itens de pedido
    /// </summary>
    public class OrderItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public string? Notes { get; set; }
        public List<OrderItemAddonResponseDto> Addons { get; set; } = new();
    }
}
