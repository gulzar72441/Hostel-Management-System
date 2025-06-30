using System;

using System.ComponentModel.DataAnnotations;

namespace HostelManagementSystemApi.Domain
{
    public class PaymentTransaction
    {
        [Key]
        public int PaymentTransactionId { get; set; }
        public int PaymentID { get; set; }
        public virtual Payment? Payment { get; set; }
        public string TransactionType { get; set; } = string.Empty; // e.g., Payment, Refund
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string GatewayTransactionID { get; set; } = string.Empty;
    }
}
