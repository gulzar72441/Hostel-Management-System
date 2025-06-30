using System.Collections.Generic;

namespace HostelManagementSystemApi.Domain
{
    public class Vendor
    {
        public int VendorID { get; set; }
        public int UserID { get; set; }
        public virtual User? User { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;

        public virtual ICollection<Hostel> Hostels { get; set; } = new List<Hostel>();
    }
}
