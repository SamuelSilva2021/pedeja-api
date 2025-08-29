using System;
using System.Collections.Generic;

namespace PedejaApp.Web.TempModels;

public partial class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public int Method { get; set; }

    public int Status { get; set; }

    public string? GatewayTransactionId { get; set; }

    public string? GatewayResponse { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? Notes { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<PaymentRefund> PaymentRefunds { get; set; } = new List<PaymentRefund>();
}
