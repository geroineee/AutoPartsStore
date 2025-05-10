using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

public partial class CustomerOrdersAggregationView
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal? TotalOrdered { get; set; }
}
