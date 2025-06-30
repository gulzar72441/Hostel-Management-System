using System;

namespace HostelManagementSystemApi.Features.Payments.DTOs
{
    public class CreatePaymentRequest
    {
        public int StudentID { get; set; }
        public int HostelID { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Unpaid";
    }
}
