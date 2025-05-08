using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Ячейка склада
/// </summary>
public partial class StorageCell
{
    public int CellId { get; set; }

    public string CellName { get; set; } = null!;

    public string? LocationDescription { get; set; }

    public decimal Width { get; set; }

    public decimal Depth { get; set; }

    public decimal Height { get; set; }

    /// <summary>
    /// Вес в кг
    /// </summary>
    public int? MaxWeight { get; set; }

    public virtual ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}
