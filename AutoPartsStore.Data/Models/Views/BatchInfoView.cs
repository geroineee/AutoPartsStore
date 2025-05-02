using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models.Views;
public partial class BatchInfoView
{
    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public int BatchId { get; set; }

    public DateTime DeliveryDate { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int ProductQuantity { get; set; }
}
