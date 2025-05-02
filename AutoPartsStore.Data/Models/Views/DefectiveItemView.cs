using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models.Views;

public partial class DefectiveItemView
{
    public int DefectId { get; set; }

    public int DefectQuantity { get; set; }

    public string ProductName { get; set; } = null!;

    public string SupplierName { get; set; } = null!;

    public DateTime BatchDeliveryDate { get; set; }
}
