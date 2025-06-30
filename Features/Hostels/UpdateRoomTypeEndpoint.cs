using FastEndpoints;
using HostelManagementSystemApi.Features.Hostels.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class UpdateRoomTypeEndpoint : Endpoint<UpdateRoomTypeRequest>
    {
        private readonly ApplicationDbContext _context;

        public UpdateRoomTypeEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/hostels/room-types/{RoomTypeID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(UpdateRoomTypeRequest req, CancellationToken ct)
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

            var roomType = await _context.RoomTypes
                .Include(rt => rt.Hostel)
                .FirstOrDefaultAsync(rt => rt.RoomTypeID == req.RoomTypeID, ct);

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

            roomType.Name = req.Name;
            roomType.Description = req.Description;
            roomType.Price = req.Price;
            roomType.Capacity = req.Capacity;
            roomType.IsActive = req.IsActive;

            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
