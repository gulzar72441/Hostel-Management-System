using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class DeleteHostelEndpoint : Endpoint<DeleteHostelRequest>
    {
        private readonly ApplicationDbContext _context;

        public DeleteHostelEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/hostels/{HostelID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(DeleteHostelRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var vendor = await _context.Vendors.AsNoTracking().FirstOrDefaultAsync(v => v.UserID == int.Parse(userId), ct);
            if (vendor == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.HostelID == req.HostelID && h.VendorID == vendor.VendorID, ct);

            if (hostel == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            _context.Hostels.Remove(hostel);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }

    public class DeleteHostelRequest
    {
        public int HostelID { get; set; }
    }
}
