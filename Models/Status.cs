using System;
using System.Collections.Generic;

namespace BigFood.Models
{
    public partial class Status
    {
        public int Id { get; set; }
        public string? Status1 { get; set; }
        public string LocationLat { get; set; } = null!;
        public string LocationLong { get; set; } = null!;
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
