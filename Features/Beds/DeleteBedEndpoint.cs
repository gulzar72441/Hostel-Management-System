using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Beds
{
    public class DeleteBedEndpoint : EndpointWithoutRequest
    {
        private readonly ApplicationDbContext _context;

        public DeleteBedEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/beds/{BedID}");
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

            var vendor = await _context.Vendors.AsNoTracking().FirstOrDefaultAsync(v => v.UserID == int.Parse(userId), ct);
            if (vendor == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var bedId = Route<int>("BedID");
            var bed = await _context.Beds
                .Include(b => b.Room)
                .ThenInclude(r => r!.Hostel)
                .FirstOrDefaultAsync(b => b.BedID == bedId, ct);

            if (bed == null || bed.Room == null || bed.Room.Hostel == null || bed.Room.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (bed.StudentID.HasValue)
            {
                AddError("Cannot delete a bed that is currently occupied.");
                await SendErrorsAsync(400, ct);
                return;
            }

            _context.Beds.Remove(bed);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
