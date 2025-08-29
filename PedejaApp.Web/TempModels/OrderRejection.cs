using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class OrderRejection
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string Reason { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime RejectedAt { get; set; }

    public string RejectedBy { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
