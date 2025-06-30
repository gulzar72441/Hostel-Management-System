using HostelManagementSystemApi.Features.Common;

namespace HostelManagementSystemApi.Features.Beds.DTOs
{
    public class GetHostelBedsRequest : PaginationRequest
    {
        public bool? IsOccupied { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }
}
