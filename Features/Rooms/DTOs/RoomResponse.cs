namespace HostelManagementSystemApi.Features.Rooms.DTOs
{
    public class RoomResponse
    {
        public int RoomID { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int RoomTypeID { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public decimal RoomTypePrice { get; set; }
        public int RoomTypeCapacity { get; set; }
    }
}
