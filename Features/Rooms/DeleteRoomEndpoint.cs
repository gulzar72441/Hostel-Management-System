using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Rooms
{
    public class DeleteRoomRequest
    {
        public int RoomID { get; set; }
    }

    public class DeleteRoomEndpoint : Endpoint<DeleteRoomRequest>
    {
        private readonly ApplicationDbContext _context;

        public DeleteRoomEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/rooms/{RoomID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(DeleteRoomRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserID == int.Parse(userId), ct);
            if (vendor == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var room = await _context.Rooms
                .Include(r => r.Hostel)
                .FirstOrDefaultAsync(r => r.RoomID == req.RoomID, ct);

            if (room == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (room.Hostel == null || room.Hostel.VendorID != vendor.VendorID)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => b.RoomID == req.RoomID && (b.Status == "Confirmed" || b.Status == "Checked-in"), ct);

            if (hasActiveBookings)
            {
                AddError("Cannot delete a room with active bookings.");
                await SendErrorsAsync(400, ct);
                return;
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
