using System;

namespace HostelManagementSystemApi.Domain
{
    public class Booking
    {
        public int BookingID { get; set; }
        public int StudentID { get; set; }
        public virtual Student? Student { get; set; }
        public int RoomID { get; set; }
        public virtual Room? Room { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., Confirmed, Pending, Cancelled
        public decimal TotalPrice { get; set; }
    }
}
