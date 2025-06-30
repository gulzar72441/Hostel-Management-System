using FastEndpoints;

namespace HostelManagementSystemApi.Features.HealthCheck
{
    public class HealthCheckEndpoint : EndpointWithoutRequest<string>
    {
        public override void Configure()
        {
            Get("/api/health");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            await SendOkAsync("API is healthy.", ct);
        }
    }
}
