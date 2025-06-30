using System;
using System.Collections.Generic;

namespace HostelManagementSystemApi.Domain
{
    public class Student
    {
        public int StudentID { get; set; }
        public int UserID { get; set; }
        public virtual User? User { get; set; }
        public int HostelID { get; set; }
        public virtual Hostel? Hostel { get; set; }
        public DateTime AdmissionDate { get; set; }
        public string KYCStatus { get; set; } = string.Empty; // e.g., Pending, Verified, Rejected
        public int? RoomID { get; set; }
        public virtual Room? Room { get; set; }
        public int? BedID { get; set; }
        public virtual Bed? Bed { get; set; }

        // Navigation properties
        public virtual ICollection<Guardian> Guardians { get; set; } = new List<Guardian>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public virtual ICollection<HostelReview> HostelReviews { get; set; } = new List<HostelReview>();
        public virtual ICollection<StudentActivityLog> StudentActivityLogs { get; set; } = new List<StudentActivityLog>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
