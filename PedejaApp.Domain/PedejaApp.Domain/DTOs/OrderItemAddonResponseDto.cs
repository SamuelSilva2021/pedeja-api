namespace PedejaApp.Web.Models.DTOs
{
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
