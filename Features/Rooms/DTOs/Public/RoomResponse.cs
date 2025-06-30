namespace HostelManagementSystemApi.Features.Rooms.DTOs.Public
{
    public class RoomResponse
    {
        public int RoomID { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty; // e.g., Single, Double, Dormitory
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}
