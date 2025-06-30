using System;

namespace HostelManagementSystemApi.Features.Payments.DTOs
{
    public class PaymentResponse
    {
        public int PaymentID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int HostelID { get; set; }
        public string HostelName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string ReceiptURL { get; set; } = string.Empty;
    }
}
