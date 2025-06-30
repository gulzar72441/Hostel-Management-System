using System;
using System.ComponentModel.DataAnnotations;

namespace HostelManagementSystemApi.Domain
{
    public class StudentActivityLog
    {
        [Key]
        public int StudentActivityLogId { get; set; }
        public int StudentID { get; set; }
        public virtual Student? Student { get; set; }
        public string ActivityType { get; set; } = string.Empty; // e.g., Login, Logout, Fee-Payment
        public DateTime Timestamp { get; set; }
    }
}
