using System;

namespace HostelManagementSystemApi.Domain
{
    public class Notice
    {
        public int NoticeID { get; set; }
        public int HostelID { get; set; }
        public virtual Hostel? Hostel { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty; // e.g., All, Students, Staff
        public DateTime Date { get; set; }
    }
}
