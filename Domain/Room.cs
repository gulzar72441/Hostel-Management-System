using System.Collections.Generic;

namespace HostelManagementSystemApi.Domain
{
    public class Room
    {
        public int RoomID { get; set; }
        public int HostelID { get; set; }
        public virtual Hostel? Hostel { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        // Foreign Key for RoomType
        public int RoomTypeID { get; set; }
        public virtual RoomType? RoomType { get; set; }

        // Navigation properties
        public virtual ICollection<Bed> Beds { get; set; } = new List<Bed>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
