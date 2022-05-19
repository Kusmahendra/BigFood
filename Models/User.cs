using System;
using System.Collections.Generic;

namespace BigFood.Models
{
    public partial class User
    {
        public User()
        {
            CourierStatuses = new HashSet<CourierStatus>();
            OrderCouriers = new HashSet<Order>();
            OrderUsers = new HashSet<Order>();
            Profiles = new HashSet<Profile>();
            UserRoles = new HashSet<UserRole>();
        }

        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;

        public virtual ICollection<CourierStatus> CourierStatuses { get; set; }
        public virtual ICollection<Order> OrderCouriers { get; set; }
        public virtual ICollection<Order> OrderUsers { get; set; }
        public virtual ICollection<Profile> Profiles { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
