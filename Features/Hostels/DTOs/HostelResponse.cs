namespace HostelManagementSystemApi.Features.Hostels.DTOs
{
    public class HostelResponse
    {
        public int HostelID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public DateTime DateListed { get; set; }
        public bool IsApproved { get; set; }
    }
}
