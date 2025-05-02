using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Возврат клиенту
/// </summary>
public partial class CustomerRefund
{
    public int CustomerRefundId { get; set; }

    public int CrSaleItemId { get; set; }

    public int CrQuantity { get; set; }

    public DateTime RefundDate { get; set; }

    public decimal RefundAmount { get; set; }

    public sbyte IsDefect { get; set; }

    public virtual SaleItem CrSaleItem { get; set; } = null!;
}
