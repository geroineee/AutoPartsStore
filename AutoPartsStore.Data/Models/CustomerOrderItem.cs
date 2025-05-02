using System;
using System.Collections.Generic;

namespace AutoPartsStore.Data.Models;

public partial class CustomerOrderItem
{
    public int CoiProductId { get; set; }

    public int CoiCustomerOrderId { get; set; }

    public int CoiQuantity { get; set; }

    public virtual CustomerOrder CoiCustomerOrder { get; set; } = null!;

    public virtual Product CoiProduct { get; set; } = null!;
}
