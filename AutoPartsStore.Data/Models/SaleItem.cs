using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Состав продажи
/// </summary>
public partial class SaleItem
{
    public int SaleItemId { get; set; }

    public int SiSaleId { get; set; }

    public int SiBatchItemId { get; set; }

    public int SiQuantity { get; set; }

    public virtual ICollection<CustomerRefund> CustomerRefunds { get; set; } = new List<CustomerRefund>();

    public virtual BatchItem SiBatchItem { get; set; } = null!;

    public virtual Sale SiSale { get; set; } = null!;
}
