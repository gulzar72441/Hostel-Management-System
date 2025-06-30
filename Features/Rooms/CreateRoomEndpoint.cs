using FastEndpoints;
using HostelManagementSystemApi.Features.Rooms.DTOs;

using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Rooms
{
    public class CreateRoomEndpoint : Endpoint<CreateRoomRequest, RoomResponse>
    {
        private readonly ApplicationDbContext _context;

        public CreateRoomEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/rooms");
            Roles("Vendor");
        }

        public override async Task HandleAsync(CreateRoomRequest req, CancellationToken ct)
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

            var hostel = await _context.Hostels.FirstOrDefaultAsync(h => h.HostelID == req.HostelID && h.VendorID == vendor.VendorID, ct);
            if (hostel == null)
            {
                AddError("Hostel not found or you do not have permission to add a room to it.");
                await SendErrorsAsync(404, ct);
                return;
            }

            var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.RoomTypeID == req.RoomTypeID && rt.HostelID == req.HostelID, ct);
            if (roomType == null)
            {
                AddError("Invalid RoomTypeID for this hostel.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var room = new Room
            {
                HostelID = req.HostelID,
                RoomNumber = req.RoomNumber,
                RoomTypeID = req.RoomTypeID,
                Description = req.Description ?? string.Empty,
                IsAvailable = req.IsAvailable
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync(ct);

            var response = new RoomResponse
            {
                RoomID = room.RoomID,
                RoomNumber = room.RoomNumber,
                Description = room.Description,
                IsAvailable = room.IsAvailable,
                RoomTypeID = roomType.RoomTypeID,
                RoomTypeName = roomType.Name,
                RoomTypePrice = roomType.Price,
                RoomTypeCapacity = roomType.Capacity
            };

            await SendAsync(response, 201, ct);
        }
    }
}
