using System.Collections.Generic;

namespace HostelManagementSystemApi.Domain
{
    public class Inventory
    {
        public int InventoryID { get; set; }
        public int HostelID { get; set; }
        public virtual Hostel? Hostel { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty; // e.g., kg, pcs, etc.

        // Navigation property
        public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
}
