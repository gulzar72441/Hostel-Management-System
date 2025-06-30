using System;
using System.ComponentModel.DataAnnotations;

namespace HostelManagementSystemApi.Domain
{
    public class InventoryLog
    {
        [Key]
        public int InventoryLogId { get; set; }
        public int InventoryID { get; set; }
        public virtual Inventory? Inventory { get; set; }
        public int StaffID { get; set; }
        public virtual Staff? Staff { get; set; }
        public string Action { get; set; } = string.Empty; // e.g., Used, Added, Removed
        public int QuantityChanged { get; set; }
        public DateTime Date { get; set; }
    }
}
