namespace HostelManagementSystemApi.Features.Hostels.DTOs
{
    public class AddHostelImageRequest
    {
        public int HostelID { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public bool IsPrimary { get; set; }
    }
}
