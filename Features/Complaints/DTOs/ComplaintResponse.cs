using System;

namespace HostelManagementSystemApi.Features.Complaints.DTOs
{
    public class ComplaintResponse
    {
        public int ComplaintID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int HostelID { get; set; }
        public string HostelName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
