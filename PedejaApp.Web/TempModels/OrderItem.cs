using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal Subtotal { get; set; }

    public string? Notes { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<OrderItemAddon> OrderItemAddons { get; set; } = new List<OrderItemAddon>();

    public virtual Product Product { get; set; } = null!;
}
