using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Application.Models
{
    // Story 2.3: Quick Operations DTOs
    public class BulkUpdateRequest
    {
        [Required]
        public List<int> ProductIds { get; set; } = new();
        
        [Required]
        public string Operation { get; set; } = string.Empty; // "toggle-availability", "update-price", "change-category"
        
        public object? Value { get; set; } // Price for price updates, CategoryId for category changes
    }

    public class BulkOperationResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<int> ProcessedIds { get; set; } = new();
        public List<int> FailedIds { get; set; } = new();
    }
}