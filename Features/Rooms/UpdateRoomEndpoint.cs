using FastEndpoints;
using HostelManagementSystemApi.Features.Rooms.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Rooms
{
    public class UpdateRoomEndpoint : Endpoint<UpdateRoomRequest>
    {
        private readonly ApplicationDbContext _context;

        public UpdateRoomEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/rooms/{RoomID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(UpdateRoomRequest req, CancellationToken ct)
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

            var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeID == req.RoomTypeID && rt.HostelID == room.HostelID, ct);
            if (roomType == null)
            {
                AddError("Invalid RoomTypeID for this hostel.");
                await SendErrorsAsync(400, ct);
                return;
            }

            room.RoomNumber = req.RoomNumber;
            room.Description = req.Description ?? room.Description;
            room.RoomTypeID = req.RoomTypeID;
            room.IsAvailable = req.IsAvailable;

            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
