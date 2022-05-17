using System;
using System.Collections.Generic;

namespace BigFood.Models
{
    public partial class OrderDetail
    {
        public int Id { get; set; }
        public bool Complete { get; set; }
        public int Quantity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int OrderId { get; set; }
        public int FoodId { get; set; }

        public virtual Food Food { get; set; } = null!;
        public virtual Order Order { get; set; } = null!;
    }
}
