using System;

namespace HostelManagementSystemApi.Features.StudentAttendances.DTOs
{
    public class MarkStudentAttendanceRequest
    {
        public int StudentID { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
    }
}
