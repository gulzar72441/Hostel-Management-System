using System;
using System.ComponentModel.DataAnnotations;

namespace HostelManagementSystemApi.Domain
{
    public class HostelReview
    {
        [Key]
        public int HostelReviewId { get; set; }
        public int HostelID { get; set; }
        public virtual Hostel? Hostel { get; set; }
        public int StudentID { get; set; }
        public virtual Student? Student { get; set; }
        public int Rating { get; set; } // e.g., 1-5
        public string Comment { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
