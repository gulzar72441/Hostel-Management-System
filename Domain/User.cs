using System;
using System.Collections.Generic;

namespace HostelManagementSystemApi.Domain
{
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public virtual Role? Role { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Vendor? Vendor { get; set; }
        public virtual Student? Student { get; set; }
        public virtual Staff? Staff { get; set; }
        public virtual Guardian? Guardian { get; set; }
        public virtual ICollection<NotificationPreference> NotificationPreferences { get; set; } = new List<NotificationPreference>();
    }
}
