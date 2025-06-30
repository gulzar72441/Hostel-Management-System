using System;

namespace HostelManagementSystemApi.Features.StudentAttendances.DTOs
{
    public class StudentAttendanceResponse
    {
        public int AttendanceID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
