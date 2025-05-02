using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

/// <summary>
/// Клиент
/// </summary>
public partial class Customer
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerSurname { get; set; } = null!;

    public string? CustomerPatronymic { get; set; }

    public string? CustomerPhone { get; set; }

    public string? CustomerEmail { get; set; }

    public virtual ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
