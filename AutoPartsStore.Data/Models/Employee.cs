using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Сотрудник
/// </summary>
public partial class Employee
{
    public int EmployeeId { get; set; }

    public int EmployeePositionId { get; set; }

    public string EmployeeName { get; set; } = null!;

    public string EmployeeSurname { get; set; } = null!;

    public string? EmployeePatronymic { get; set; }

    public string? EmployeePhone { get; set; }

    public string? EmployeeEmail { get; set; }

    public DateOnly? HireDate { get; set; }

    public DateOnly? FireDate { get; set; }

    public virtual Position EmployeePosition { get; set; } = null!;

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<SupplierOrder> SupplierOrders { get; set; } = new List<SupplierOrder>();
}
