using System;
using System.Collections.Generic;

namespace SellApp.Model;

public partial class Order
{
    public int OrderId { get; set; }

    public string Customer { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();
}
