using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Состав заказа
/// </summary>
public partial class SupplierOrderItem
{
    public int SoiSupplierOrderId { get; set; }

    public int SoiProductId { get; set; }

    public int SoiQuantity { get; set; }

    public virtual Product SoiProduct { get; set; } = null!;

    public virtual SupplierOrder SoiSupplierOrder { get; set; } = null!;
}
