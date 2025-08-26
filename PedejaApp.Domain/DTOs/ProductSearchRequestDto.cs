using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Domain.DTOs
{
    public class ProductSearchRequestDto
    {
        public string? SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
}
