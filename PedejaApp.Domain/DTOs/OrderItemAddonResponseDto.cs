namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO de resposta para adicionais de itens de pedido
    /// </summary>
    public class OrderItemAddonResponseDto
    {
        public int Id { get; set; }
        public int AddonId { get; set; }
        public string AddonName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}
