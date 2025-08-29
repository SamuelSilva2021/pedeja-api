using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class Order
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string? CustomerEmail { get; set; }

    public string DeliveryAddress { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal DeliveryFee { get; set; }

    public decimal Total { get; set; }

    public int Status { get; set; }

    public string? Notes { get; set; }

    public bool IsDelivery { get; set; }

    public int? EstimatedPreparationMinutes { get; set; }

    public DateTime? EstimatedDeliveryTime { get; set; }

    public int QueuePosition { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual OrderRejection? OrderRejection { get; set; }

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
