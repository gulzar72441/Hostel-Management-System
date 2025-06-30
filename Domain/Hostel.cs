using System.Collections.Generic;

namespace HostelManagementSystemApi.Domain
{
    public class Hostel
    {
        public int HostelID { get; set; }
        public int VendorID { get; set; }
        public virtual Vendor? Vendor { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public DateTime DateListed { get; set; }
        public bool IsApproved { get; set; }
        public string Type { get; set; } = string.Empty; // e.g., Boys, Girls, Co-ed
        public string GeoLocation { get; set; } = string.Empty; // e.g., "latitude,longitude"


        public bool IsActive { get; set; }

        // Navigation properties
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Notice> Notices { get; set; } = new List<Notice>();
        public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
        public virtual ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
        public virtual ICollection<HostelReview> HostelReviews { get; set; } = new List<HostelReview>();
        public virtual ICollection<HostelImage> HostelImages { get; set; } = new List<HostelImage>();
        public virtual ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();
        public virtual ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
    }
}
