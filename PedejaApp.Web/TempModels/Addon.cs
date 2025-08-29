using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class Addon
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int AddonGroupId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AddonGroup AddonGroup { get; set; } = null!;

    public virtual ICollection<OrderItemAddon> OrderItemAddons { get; set; } = new List<OrderItemAddon>();
}
