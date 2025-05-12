using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Заказ поставщику
/// </summary>
public partial class SupplierOrder
{
    public int SupplierOrderId { get; set; }

    public int ManagerId { get; set; }

    public int RecipientId { get; set; }

    public DateTime SupplierOrderDate { get; set; }

    public DateOnly? ExpectedDeliveryDate { get; set; }

    public string? SupplierOrderDescription { get; set; }

    public bool SupplierOrderStatus { get; set; }

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

    public virtual Employee Manager { get; set; } = null!;

    public virtual Supplier Recipient { get; set; } = null!;

    public virtual ICollection<SupplierOrderItem> SupplierOrderItems { get; set; } = new List<SupplierOrderItem>();
}
