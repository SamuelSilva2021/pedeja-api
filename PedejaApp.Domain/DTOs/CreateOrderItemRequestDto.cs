using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO para criação de itens de pedido
    /// </summary>
    public class CreateOrderItemRequestDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id do produto precisa ser maior que zero")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 99, ErrorMessage = "Quantidade deve estar entre 1 e 99")]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
        
        public List<CreateOrderItemAddonRequestDto> Addons { get; set; } = new();
    }
}
