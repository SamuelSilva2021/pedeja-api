using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Domain.DTOs
{
    public class CreateProductRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }
    }
}
