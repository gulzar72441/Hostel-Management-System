using FastEndpoints;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Features.Hostels.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class GetMyHostelsEndpoint : EndpointWithoutRequest<List<VendorHostelResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetMyHostelsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/hostels/my-hostels");
            Roles("Vendor");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var vendor = await _context.Vendors.AsNoTracking()
                .FirstOrDefaultAsync(v => v.UserID == int.Parse(userId), ct);

            if (vendor == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var hostels = await _context.Hostels.AsNoTracking()
                .Where(h => h.VendorID == vendor.VendorID)
                .Select(h => new VendorHostelResponse
                {
                    HostelID = h.HostelID,
                    Name = h.Name,
                    Address = h.Address,
                    City = h.City,
                    IsApproved = h.IsApproved,
                    IsActive = h.IsActive,
                    DateListed = h.DateListed
                })
                .ToListAsync(ct);

            await SendAsync(hostels, 200, ct);
        }
    }
}
