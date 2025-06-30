using System;

namespace HostelManagementSystemApi.Features.Notices.DTOs
{
    public class NoticeResponse
    {
        public int NoticeID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string HostelName { get; set; } = string.Empty;
    }
}
