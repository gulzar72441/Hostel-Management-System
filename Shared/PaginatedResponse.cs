using System.Collections.Generic;

namespace HostelManagementSystemApi.Shared
{
    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long TotalCount { get; set; }
        public bool HasNextPage => (long)Page * PageSize < TotalCount;
        public bool HasPreviousPage => Page > 1;
    }
}
