using System;
using System.Collections.Generic;

namespace BigFood.Models
{
    public partial class Profile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public virtual User User { get; set; } = null!;
    }
}
