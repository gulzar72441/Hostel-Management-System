using System;

namespace HostelManagementSystemApi.Features.Reviews.DTOs
{
    public class HostelReviewResponse
    {
        public int HostelReviewId { get; set; }
        public int HostelID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
