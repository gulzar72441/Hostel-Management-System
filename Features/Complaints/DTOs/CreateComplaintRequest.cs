namespace HostelManagementSystemApi.Features.Complaints.DTOs
{
    public class CreateComplaintRequest
    {
        public int HostelID { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
