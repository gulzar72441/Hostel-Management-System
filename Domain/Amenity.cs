using System.Collections.Generic;

namespace HostelManagementSystemApi.Domain
{
    public class Amenity
    {
        public int AmenityID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public virtual ICollection<Hostel> Hostels { get; set; } = new List<Hostel>();
    }
}
