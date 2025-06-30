using FastEndpoints;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Features.Hostels.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class AddRoomTypeEndpoint : Endpoint<RoomTypeRequest>
    {
        private readonly ApplicationDbContext _context;

        public AddRoomTypeEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/hostels/room-types");
            Roles("Vendor");
        }

        public override async Task HandleAsync(RoomTypeRequest req, CancellationToken ct)
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
                .FirstOrDefaultAsync(h => h.HostelID == req.HostelID && h.VendorID == vendor.VendorID, ct);

            if (hostel == null)
            {
                await SendAsync(new { Message = "Hostel not found or you do not have permission to manage it." }, 404, ct);
                return;
            }

            var roomType = new RoomType
            {
                HostelID = req.HostelID,
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                Capacity = req.Capacity,
                IsActive = req.IsActive
            };

            await _context.RoomTypes.AddAsync(roomType, ct);
            await _context.SaveChangesAsync(ct);

            await SendCreatedAtAsync<AddRoomTypeEndpoint>(new { roomTypeId = roomType.RoomTypeID }, null, cancellation: ct);
        }
    }
}
