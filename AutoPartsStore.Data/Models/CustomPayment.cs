using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Таможенный платеж
/// </summary>
public partial class CustomPayment
{
    public int CustomPaymentId { get; set; }

    public int CpBatchId { get; set; }

    public decimal PaymentAmount { get; set; }

    public DateOnly PaymentDateDate { get; set; }

    public virtual Batch CpBatch { get; set; } = null!;
}
