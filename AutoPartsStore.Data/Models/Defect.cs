using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Брак
/// </summary>
public partial class Defect
{
    public int DefectId { get; set; }

    public int DefectBatchItemId { get; set; }

    public DateTime DetectionDate { get; set; }

    public int DefectQuantity { get; set; }

    public string DefectDescription { get; set; } = null!;

    public virtual BatchItem DefectBatchItem { get; set; } = null!;

    public virtual ICollection<SupplierReturn> SupplierReturns { get; set; } = new List<SupplierReturn>();
}
