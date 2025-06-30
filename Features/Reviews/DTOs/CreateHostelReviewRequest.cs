namespace HostelManagementSystemApi.Features.Reviews.DTOs
{
    public class CreateHostelReviewRequest
    {
        public int HostelID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
