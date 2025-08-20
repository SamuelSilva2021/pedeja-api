namespace PedejaApp.Web.Models.DTOs
{
    /// <summary>
    /// Representa um item de pedido.
    /// </summary>
    public class OrderItemDto
    {
        /// <summary>
        /// Identificador único do item de pedido.
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Nome do produto do item de pedido.
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Preço unitário do item de pedido.
        /// </summary>
        public string? Notes { get; set; }
        /// <summary>
        /// Lista de complementos (addons) associados ao item de pedido.
        /// </summary>
        public List<OrderItemAddonDto> Addons { get; set; } = new();
    }
}
