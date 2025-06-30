using System;

namespace HostelManagementSystemApi.Features.Bookings.DTOs
{
    public class CreateBookingRequest
    {
        public int RoomID { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}
