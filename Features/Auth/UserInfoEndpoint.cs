using FastEndpoints;
using HostelManagementSystemApi.Features.Auth.DTOs;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Auth
{
    public class UserInfoEndpoint : EndpointWithoutRequest<UserInfoResponse>
    {
        public override void Configure()
        {
            Get("/api/auth/userinfo");
            Roles("Admin", "Vendor", "Student");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userInfo = new UserInfoResponse
            {
                UserId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value,
                Email = User.Claims.First(c => c.Type == ClaimTypes.Email).Value,
                Name = User.Claims.First(c => c.Type == ClaimTypes.Name).Value,
                Roles = User.FindAll(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            };

            await SendAsync(userInfo, cancellation: ct);
        }
    }
}
