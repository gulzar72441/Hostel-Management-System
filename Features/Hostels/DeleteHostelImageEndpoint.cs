using FastEndpoints;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class DeleteHostelImageEndpoint : EndpointWithoutRequest
    {
        private readonly ApplicationDbContext _context;

        public DeleteHostelImageEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/hostels/images/{HostelImageID}");
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

            var imageId = Route<int>("HostelImageID");

            var image = await _context.HostelImages
                .Include(i => i.Hostel)
                .FirstOrDefaultAsync(i => i.HostelImageID == imageId, ct);

            if (image == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (image.Hostel?.VendorID != vendor.VendorID)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            _context.HostelImages.Remove(image);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
