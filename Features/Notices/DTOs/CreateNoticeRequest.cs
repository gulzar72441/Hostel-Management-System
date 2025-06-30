namespace HostelManagementSystemApi.Features.Notices.DTOs
{
    public class CreateNoticeRequest
    {
        public int HostelID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
