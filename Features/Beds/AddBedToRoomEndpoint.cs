using FastEndpoints;
using HostelManagementSystemApi.Features.Beds.DTOs;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Beds
{
    public class AddBedToRoomEndpoint : Endpoint<CreateBedRequest, BedResponse>
    {
        private readonly ApplicationDbContext _context;

        public AddBedToRoomEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/beds");
            Roles("Vendor");
        }

        public override async Task HandleAsync(CreateBedRequest req, CancellationToken ct)
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

            if (room == null || room.Hostel == null || room.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var bed = new Bed
            {
                RoomID = req.RoomID,
                BedNumber = req.BedNumber
            };

            _context.Beds.Add(bed);
            await _context.SaveChangesAsync(ct);

            var response = new BedResponse
            {
                BedID = bed.BedID,
                BedNumber = bed.BedNumber,
                RoomID = bed.RoomID
            };

            await SendAsync(response, 201, ct);
        }
    }
}
