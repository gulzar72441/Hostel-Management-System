using System;
using System.Collections.Generic;

namespace HostelManagementSystemApi.Domain
{
    public class Staff
    {
        public int StaffID { get; set; }
        public int UserID { get; set; }
        public virtual User? User { get; set; }
        public int HostelID { get; set; }
        public virtual Hostel? Hostel { get; set; }
        public decimal Salary { get; set; }
        public DateTime JoinDate { get; set; }

        // Navigation properties
        public virtual ICollection<StaffAttendance> StaffAttendances { get; set; } = new List<StaffAttendance>();
        public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
}
