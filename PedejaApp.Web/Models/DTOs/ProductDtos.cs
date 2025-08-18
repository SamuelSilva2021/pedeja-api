using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Web.Models.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        
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
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        // Category information
        public string CategoryName { get; set; } = string.Empty;
    }

    public class CreateProductRequest
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

    public class UpdateProductRequest
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

    public class ReorderProductsRequest
    {
        [Required]
        public Dictionary<int, int> ProductOrders { get; set; } = new();
    }

    public class UpdateProductPriceRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }
    }

    public class ProductSearchRequest
    {
        public string? SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }
    }

    // Story 2.3: Quick Operations DTOs
    public class ToggleAvailabilityResponse
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class QuickPriceUpdateRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }
    }

    public class QuickPriceUpdateResponse
    {
        public int Id { get; set; }
        public decimal PreviousPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

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

    public class BulkOperationResponse
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<int> ProcessedIds { get; set; } = new();
        public List<int> FailedIds { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class PriceHistoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal PreviousPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public string? Reason { get; set; }
    }
}
