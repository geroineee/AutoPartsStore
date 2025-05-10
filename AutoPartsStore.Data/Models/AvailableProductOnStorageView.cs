using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

public partial class AvailableProductOnStorageView
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int BatchItemId { get; set; }

    public int? CurrentQuantity { get; set; }

    public int CellId { get; set; }

    public string CellName { get; set; } = null!;

    public int? QuantityInCell { get; set; }

    public int BatchId { get; set; }
}
