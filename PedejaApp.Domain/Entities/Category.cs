using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Domain.Entities;

public class Category : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property for products
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
