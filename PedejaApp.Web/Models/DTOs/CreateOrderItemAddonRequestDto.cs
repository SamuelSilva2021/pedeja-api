using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Web.Models.DTOs
{
    public class CreateOrderItemAddonRequestDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id do adicional deve ser maior que zero")]
        public int AddonId { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Quantidade do adicional deve estar entre 1 e 10")]
        public int Quantity { get; set; } = 1;
    }
}
