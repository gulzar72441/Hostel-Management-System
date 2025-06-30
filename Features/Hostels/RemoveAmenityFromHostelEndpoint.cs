using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class RemoveAmenityFromHostelEndpoint : EndpointWithoutRequest
    {
        private readonly ApplicationDbContext _context;

        public RemoveAmenityFromHostelEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/hostels/{HostelID}/amenities/{AmenityID}");
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

            var hostelId = Route<int>("HostelID");
            var amenityId = Route<int>("AmenityID");

            var hostel = await _context.Hostels
                .Include(h => h.Amenities)
                .FirstOrDefaultAsync(h => h.HostelID == hostelId && h.VendorID == vendor.VendorID, ct);

            if (hostel == null)
            {
                await SendAsync(new { Message = "Hostel not found or you do not have permission to manage it." }, 404, ct);
                return;
            }

            var amenity = hostel.Amenities.FirstOrDefault(a => a.AmenityID == amenityId);
            if (amenity == null)
            {
                await SendAsync(new { Message = "Amenity not found for this hostel." }, 404, ct);
                return;
            }

            hostel.Amenities.Remove(amenity);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
