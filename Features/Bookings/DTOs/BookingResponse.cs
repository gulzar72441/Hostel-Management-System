using System;

namespace HostelManagementSystemApi.Features.Bookings.DTOs
{
    public class BookingResponse
    {
        public int BookingID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int HostelID { get; set; }
        public string HostelName { get; set; } = string.Empty;
        public int RoomID { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
    }
}
