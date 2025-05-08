using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Партия
/// </summary>
public partial class Batch
{
    public int BatchId { get; set; }

    public int BatchSupplierId { get; set; }

    public DateTime DeliveryDate { get; set; }

    public string? BatchDescription { get; set; }

    public virtual ICollection<BatchItem> BatchItems { get; set; } = new List<BatchItem>();

    public virtual Supplier BatchSupplier { get; set; } = null!;

    public virtual ICollection<CustomPayment> CustomPayments { get; set; } = new List<CustomPayment>();
}
