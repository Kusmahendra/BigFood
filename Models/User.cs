using System;
using System.Collections.Generic;

namespace BigFood.Models
{
    public partial class User
    {
        public User()
        {
            Profiles = new HashSet<Profile>();
            Statuses = new HashSet<Status>();
            UserRoles = new HashSet<UserRole>();
        }

        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;

        public virtual ICollection<Profile> Profiles { get; set; }
        public virtual ICollection<Status> Statuses { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
