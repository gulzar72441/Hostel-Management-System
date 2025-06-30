namespace HostelManagementSystemApi.Features.Payments.DTOs
{
    public class VendorPaymentResponse
    {
        public int PaymentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string HostelName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
    }
}
