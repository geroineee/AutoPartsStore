using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

public partial class SupplierReturn
{
    public int SupplierReturnId { get; set; }

    public int SrDefectId { get; set; }

    public DateOnly ReturnDate { get; set; }

    public int ReturnQuantity { get; set; }

    public decimal CompensationAmount { get; set; }

    public bool? ReplacementCompensation { get; set; }

    public virtual Defect SrDefect { get; set; } = null!;
}
