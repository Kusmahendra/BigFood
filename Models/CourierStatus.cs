using System;
using System.Collections.Generic;

namespace BigFood.Models
{
    public partial class CourierStatus
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public string? LocationLat { get; set; }
        public string? LocationLong { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
