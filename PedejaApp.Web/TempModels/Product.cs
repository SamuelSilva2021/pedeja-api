using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductAddonGroup> ProductAddonGroups { get; set; } = new List<ProductAddonGroup>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
