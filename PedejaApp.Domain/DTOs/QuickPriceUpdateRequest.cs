using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Domain.DTOs
{
    public class QuickPriceUpdateRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }
    }
}