namespace HostelManagementSystemApi.Features.HostelReviews.DTOs
{
    public class UpdateHostelReviewRequest
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
