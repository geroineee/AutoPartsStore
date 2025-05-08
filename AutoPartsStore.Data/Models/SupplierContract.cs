using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Контракт поставщика
/// </summary>
public partial class SupplierContract
{
    public int ContractId { get; set; }

    public int ContractSupplierId { get; set; }

    public string ContractNumber { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Terms { get; set; }

    public decimal Discount { get; set; }

    public virtual Supplier ContractSupplier { get; set; } = null!;
}
