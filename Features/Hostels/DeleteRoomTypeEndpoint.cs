using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class DeleteRoomTypeEndpoint : EndpointWithoutRequest
    {
        private readonly ApplicationDbContext _context;

        public DeleteRoomTypeEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/hostels/room-types/{RoomTypeID}");
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

            var roomTypeId = Route<int>("RoomTypeID");

            var roomType = await _context.RoomTypes
                .Include(rt => rt.Hostel)
                .FirstOrDefaultAsync(rt => rt.RoomTypeID == roomTypeId, ct);

            if (roomType == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (roomType.Hostel?.VendorID != vendor.VendorID)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
