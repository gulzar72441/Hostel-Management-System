using System;
using System.Collections.Generic;

namespace HostelManagementSystemApi.Domain
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int StudentID { get; set; }
        public virtual Student? Student { get; set; }
        public int HostelID { get; set; }
        public virtual Hostel? Hostel { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., Paid, Unpaid, Overdue
        public string Method { get; set; } = string.Empty; // e.g., Cash, Online
        public string ReceiptURL { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
}
