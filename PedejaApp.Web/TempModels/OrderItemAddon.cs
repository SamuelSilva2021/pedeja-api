using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class OrderItemAddon
{
    public int Id { get; set; }

    public int OrderItemId { get; set; }

    public int AddonId { get; set; }

    public string AddonName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Addon Addon { get; set; } = null!;

    public virtual OrderItem OrderItem { get; set; } = null!;
}
