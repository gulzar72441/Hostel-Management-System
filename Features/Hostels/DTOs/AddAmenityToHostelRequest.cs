namespace HostelManagementSystemApi.Features.Hostels.DTOs
{
    public class AddAmenityToHostelRequest
    {
        public int HostelID { get; set; }
        public int AmenityID { get; set; }
    }
}
