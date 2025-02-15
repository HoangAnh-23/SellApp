using System;
using System.Collections.Generic;

namespace SellApp.Model;

public partial class Orderdetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? UnitBill { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }


    public int Stt { get; set; }

    public decimal TotalPrice
    {
        get { return Quantity * UnitPrice; }
        set { } // Setter trống (hoặc logic khác nếu cần)
    }

    public virtual Order Order { get; set; } = null!;
}
