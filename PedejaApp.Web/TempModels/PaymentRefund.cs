using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class PaymentRefund
{
    public int Id { get; set; }

    public int PaymentId { get; set; }

    public decimal Amount { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime RefundedAt { get; set; }

    public string RefundedBy { get; set; } = null!;

    public string? GatewayRefundId { get; set; }

    public string? GatewayResponse { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
