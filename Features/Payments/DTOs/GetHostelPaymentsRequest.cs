using HostelManagementSystemApi.Features.Common;

namespace HostelManagementSystemApi.Features.Payments.DTOs
{
    public class GetHostelPaymentsRequest : PaginationRequest
    {
        public string? Status { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }
}
