using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models.Views;

public partial class SupplierProductStatView
{
    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public int SupplierCategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal DeliveryPrice { get; set; }

    public int DeliveryDays { get; set; }

    public long TotalDeliveries { get; set; }

    public decimal? TotalQuantity { get; set; }
}
