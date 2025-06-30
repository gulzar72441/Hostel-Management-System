using System;

namespace HostelManagementSystemApi.Features.Payments.DTOs
{
    public class UpdatePaymentRequest
    {
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public DateTime? PaidDate { get; set; }
    }
}
