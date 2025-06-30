namespace HostelManagementSystemApi.Features.Hostels.DTOs
{
    public class VendorHostelResponse
    {
        public int HostelID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateListed { get; set; }
    }
}
