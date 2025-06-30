using System.Collections.Generic;

namespace HostelManagementSystemApi.Features.Auth.DTOs
{
    public class UserInfoResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}
