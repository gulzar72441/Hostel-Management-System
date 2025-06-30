using HostelManagementSystemApi.Features.Common;

namespace HostelManagementSystemApi.Features.Staff.DTOs
{
    public class GetHostelStaffRequest : PaginationRequest
    {
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }
}
