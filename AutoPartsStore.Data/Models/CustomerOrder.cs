using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

public partial class CustomerOrder
{
    public int CustomerOrderId { get; set; }

    public int CoCustomerId { get; set; }

    public DateTime CustomerOrderDate { get; set; }

    public bool CustomerOrderStatus { get; set; }

    public string? CustomerOrderDescription { get; set; }

    public virtual Customer CoCustomer { get; set; } = null!;

    public virtual ICollection<CustomerOrderItem> CustomerOrderItems { get; set; } = new List<CustomerOrderItem>();
}
