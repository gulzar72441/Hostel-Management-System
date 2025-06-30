namespace HostelManagementSystemApi.Domain
{
    public class AttendanceReason
    {
        public int AttendanceReasonId { get; set; }
        public int AttendanceID { get; set; }
        public virtual Attendance? Attendance { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
