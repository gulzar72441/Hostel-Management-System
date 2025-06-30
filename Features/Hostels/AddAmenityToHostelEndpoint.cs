using FastEndpoints;
using HostelManagementSystemApi.Features.Hostels.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class AddAmenityToHostelEndpoint : Endpoint<AddAmenityToHostelRequest>
    {
        private readonly ApplicationDbContext _context;

        public AddAmenityToHostelEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/hostels/amenities");
            Roles("Vendor");
        }

        public override async Task HandleAsync(AddAmenityToHostelRequest req, CancellationToken ct)
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

            var hostel = await _context.Hostels
                .Include(h => h.Amenities)
                .FirstOrDefaultAsync(h => h.HostelID == req.HostelID && h.VendorID == vendor.VendorID, ct);

            if (hostel == null)
            {
                await SendAsync(new { Message = "Hostel not found or you do not have permission to manage it." }, 404, ct);
                return;
            }

            var amenity = await _context.Amenities.FindAsync(req.AmenityID);
            if (amenity == null)
            {
                await SendAsync(new { Message = "Amenity not found." }, 404, ct);
                return;
            }

            if (hostel.Amenities.Any(a => a.AmenityID == req.AmenityID))
            {
                await SendAsync(new { Message = "Amenity already exists for this hostel." }, 409, ct);
                return;
            }

            hostel.Amenities.Add(amenity);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
