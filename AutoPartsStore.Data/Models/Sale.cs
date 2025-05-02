using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Продажа
/// </summary>
public partial class Sale
{
    public int SaleId { get; set; }

    public int SaleCustomerId { get; set; }

    public int SaleEmployeeId { get; set; }

    public DateTime SaleDate { get; set; }

    public decimal SaleTotalAmount { get; set; }

    public virtual Customer SaleCustomer { get; set; } = null!;

    public virtual Employee SaleEmployee { get; set; } = null!;

    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
