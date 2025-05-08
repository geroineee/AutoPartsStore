using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Категория поставщика
/// </summary>
public partial class SupplierCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public bool ProvidesGuarantee { get; set; }

    public bool ProvidesDiscount { get; set; }

    public string? CategoryDescription { get; set; }

    public virtual ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
}
