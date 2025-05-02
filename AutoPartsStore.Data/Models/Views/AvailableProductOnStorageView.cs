using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models.Views;

public partial class AvailableProductOnStorageView
{
    public int BatchItemId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int? RemainingItem { get; set; }

    public string StockStorageCellName { get; set; } = null!;

    public DateTime DeliveryDate { get; set; }
}
