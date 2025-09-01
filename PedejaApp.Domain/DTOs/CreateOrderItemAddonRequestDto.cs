using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO para criação de adicionais de itens de pedido
    /// </summary>
    public class CreateOrderItemAddonRequestDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id do addon precisa ser maior que 0")]
        public int AddonId { get; set; }

        [Required]
        [Range(1, 99, ErrorMessage = "Quantidade deve estar entre 1 e 99")]
        public int Quantity { get; set; }
    }
}
