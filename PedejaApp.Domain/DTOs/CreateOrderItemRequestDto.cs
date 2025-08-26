using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Web.Models.DTOs
{
    public class CreateOrderItemRequestDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id do produto precisa ser maior que zero 0")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 99, ErrorMessage = "Quantidade deve estar entre 1 e 99")]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
        public List<CreateOrderItemAddonRequestDto> Addons { get; set; } = new();
    }
}
