using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class AddonGroup
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Type { get; set; }

    public int? MinSelections { get; set; }

    public int? MaxSelections { get; set; }

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Addon> Addons { get; set; } = new List<Addon>();

    public virtual ICollection<ProductAddonGroup> ProductAddonGroups { get; set; } = new List<ProductAddonGroup>();
}
