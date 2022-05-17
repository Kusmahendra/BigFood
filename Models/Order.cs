﻿using System;
using System.Collections.Generic;

namespace BigFood.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourierId { get; set; }
        public int Distance { get; set; }

        public virtual User Courier { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}