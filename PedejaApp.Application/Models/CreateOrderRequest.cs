using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Application.Models;

public class CreateOrderRequest
{
    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Phone]
    public string CustomerPhone { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string? CustomerEmail { get; set; }

    [Required]
    [StringLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool IsDelivery { get; set; } = true;

    [Required]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
