using FastEndpoints;
using HostelManagementSystemApi.Features.Admin.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HostelManagementSystemApi.Features.Admin
{
    public class GetPendingHostelsEndpoint : EndpointWithoutRequest<List<PendingHostelResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetPendingHostelsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/admin/hostels/pending-approval");
            Roles("Admin");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var pendingHostels = await _context.Hostels
                .Where(h => !h.IsApproved && h.Vendor != null && h.Vendor.User != null)
                .Select(h => new PendingHostelResponse
                {
                    HostelID = h.HostelID,
                    Name = h.Name,
                    City = h.City,
                    State = h.State,
                    DateListed = h.DateListed,
                    VendorID = h.VendorID,
                    VendorName = h.Vendor!.User!.Name
                })
                .ToListAsync(ct);

            await SendAsync(pendingHostels, 200, ct);
        }
    }
}
