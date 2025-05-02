using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models.Views;

public partial class CustomerPurchaseStatView
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerSurname { get; set; } = null!;

    public string? CustomerPatronymic { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal? TotalPurchased { get; set; }

    public decimal? TotalSpent { get; set; }
}
