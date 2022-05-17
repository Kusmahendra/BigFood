using System;
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
        public string Distance { get; set; } = null!;
        public bool? Complete { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
