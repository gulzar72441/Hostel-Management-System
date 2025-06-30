namespace HostelManagementSystemApi.Features.Hostels.DTOs
{
    public class RoomTypeRequest
    {
        public int HostelID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
