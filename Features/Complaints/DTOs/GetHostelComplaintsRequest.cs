using HostelManagementSystemApi.Features.Common;

namespace HostelManagementSystemApi.Features.Complaints.DTOs
{
    public class GetHostelComplaintsRequest : PaginationRequest
    {
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }
}
