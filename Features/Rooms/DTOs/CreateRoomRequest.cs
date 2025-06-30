namespace HostelManagementSystemApi.Features.Rooms.DTOs
{
    public class CreateRoomRequest
    {
        public int HostelID { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int RoomTypeID { get; set; }
        public bool IsAvailable { get; set; }
    }
}
