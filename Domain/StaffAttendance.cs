using System;

namespace HostelManagementSystemApi.Domain
{
    public class StaffAttendance
    {
        public int StaffAttendanceID { get; set; }
        public int StaffID { get; set; }
        public virtual Staff? Staff { get; set; }
        public DateTime Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., Present, Absent, On-Leave
    }
}
