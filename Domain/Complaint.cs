using System;

namespace HostelManagementSystemApi.Domain
{
    public class Complaint
    {
        public int ComplaintID { get; set; }
        public int StudentID { get; set; }
        public virtual Student? Student { get; set; }
        public int HostelID { get; set; }
        public virtual Hostel? Hostel { get; set; }
        public string Type { get; set; } = string.Empty; // e.g., Maintenance, Food, etc.
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // e.g., Pending, In-Progress, Resolved
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
