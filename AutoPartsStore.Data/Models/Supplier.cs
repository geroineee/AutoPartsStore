using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Поставщик
/// </summary>
public partial class Supplier
{
    public int SupplierId { get; set; }

    public int SupplierCategoryId { get; set; }

    public string SupplierName { get; set; } = null!;

    public string SupplierAddress { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

    public virtual ICollection<DeliveryTerm> DeliveryTerms { get; set; } = new List<DeliveryTerm>();

    public virtual SupplierCategory SupplierCategory { get; set; } = null!;

    public virtual ICollection<SupplierContract> SupplierContracts { get; set; } = new List<SupplierContract>();

    public virtual ICollection<SupplierOrder> SupplierOrders { get; set; } = new List<SupplierOrder>();
}
