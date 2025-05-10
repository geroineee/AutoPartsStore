using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

public partial class Position
{
    public int PositionId { get; set; }

    public string PositionName { get; set; } = null!;

    public string? PositionDescription { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
