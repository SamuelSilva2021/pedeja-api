using PedejaApp.Web.Controllers;
using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Web.Models.DTOs
{
    public class CreateOrderRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 10)]
        public string CustomerPhone { get; set; } = string.Empty;

        [EmailAddress]
        public string? CustomerEmail { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string DeliveryAddress { get; set; } = string.Empty;

        public bool IsDelivery { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CreateOrderItemRequestDto> Items { get; set; } = new();
    }
}
