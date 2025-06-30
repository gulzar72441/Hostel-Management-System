using System;

namespace HostelManagementSystemApi.Features.StaffAttendances.DTOs
{
    public class StaffAttendanceResponse
    {
        public int StaffAttendanceID { get; set; }
        public int StaffID { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
