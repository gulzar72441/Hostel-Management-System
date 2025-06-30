using HostelManagementSystemApi.Features.Common;

namespace HostelManagementSystemApi.Features.Reviews.DTOs
{
    public class GetHostelReviewsRequest : PaginationRequest
    {
        public int? Rating { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }
}
