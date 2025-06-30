using System;

namespace HostelManagementSystemApi.Features.StaffAttendances.DTOs
{
    public class MarkAttendanceRequest
    {
        public int StaffID { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
    }
}
