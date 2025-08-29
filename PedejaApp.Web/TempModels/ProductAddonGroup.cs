using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class ProductAddonGroup
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int AddonGroupId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsRequired { get; set; }

    public int? MinSelectionsOverride { get; set; }

    public int? MaxSelectionsOverride { get; set; }

    public virtual AddonGroup AddonGroup { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
