namespace HostelManagementSystemApi.Features.Notices.DTOs
{
    public class UpdateNoticeRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
