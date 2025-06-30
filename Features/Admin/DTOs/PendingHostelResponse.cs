namespace HostelManagementSystemApi.Features.Admin.DTOs
{
    public class PendingHostelResponse
    {
        public int HostelID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public DateTime DateListed { get; set; }
        public int VendorID { get; set; }
        public string VendorName { get; set; } = string.Empty;
    }
}
