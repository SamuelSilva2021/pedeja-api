using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int Status { get; set; }

    public DateTime Timestamp { get; set; }

    public string? Notes { get; set; }

    public string UserId { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
