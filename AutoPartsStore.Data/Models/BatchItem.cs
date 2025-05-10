using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Состав партии
/// </summary>
public partial class BatchItem
{
    public int BatchItemId { get; set; }

    public int BiProductId { get; set; }

    public int BiBatchId { get; set; }

    public int BatchItemQuantity { get; set; }

    public int? RemainingItem { get; set; }

    public int BiCellId { get; set; }

    public virtual Batch BiBatch { get; set; } = null!;

    public virtual StorageCell BiCell { get; set; } = null!;

    public virtual Product BiProduct { get; set; } = null!;

    public virtual ICollection<Defect> Defects { get; set; } = new List<Defect>();

    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
