namespace HostelManagementSystemApi.Features.HostelReviews.DTOs
{
    public class AddHostelReviewRequest
    {
        public int HostelID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
