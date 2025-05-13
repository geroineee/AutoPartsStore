using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

public partial class SaleWithSupplierInfoView
{
    public int SaleItemId { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public DateTime SaleDate { get; set; }

    public int SaleQuantity { get; set; }

    public decimal SalePrice { get; set; }

    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public decimal PurchasePrice { get; set; }

    public decimal Profit { get; set; }
}
