using System;
using System.Collections.Generic;

namespace HostelManagementSystemApi.Domain
{
    public class Attendance
    {
        public int AttendanceID { get; set; }
        public int StudentID { get; set; }
        public virtual Student? Student { get; set; }
        public DateTime Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., Present, Absent, On-Leave

        // Navigation property
        public virtual ICollection<AttendanceReason> AttendanceReasons { get; set; } = new List<AttendanceReason>();
    }
}
