using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Условия поставки
/// </summary>
public partial class DeliveryTerm
{
    public int DtSupplierId { get; set; }

    public int DtProductId { get; set; }

    public decimal DeliveryPrice { get; set; }

    public int DeliveryDays { get; set; }

    public virtual Product DtProduct { get; set; } = null!;

    public virtual Supplier DtSupplier { get; set; } = null!;
}
