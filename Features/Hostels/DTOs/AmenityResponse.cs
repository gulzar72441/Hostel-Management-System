namespace HostelManagementSystemApi.Features.Hostels.DTOs
{
    public class AmenityResponse
    {
        public int AmenityID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
