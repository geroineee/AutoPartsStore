using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Товар на складе
/// </summary>
public partial class StockItem
{
    public int StockBatchItemId { get; set; }

    public string StockStorageCellName { get; set; } = null!;

    public int StockItemQuantity { get; set; }

    public virtual BatchItem StockBatchItem { get; set; } = null!;

    public virtual StorageCell StockStorageCellNameNavigation { get; set; } = null!;
}
