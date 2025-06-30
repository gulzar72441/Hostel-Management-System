namespace HostelManagementSystemApi.Features.Rooms.DTOs
{
    public class UpdateRoomRequest
    {
        public int RoomID { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int RoomTypeID { get; set; }
        public bool IsAvailable { get; set; }
    }
}
