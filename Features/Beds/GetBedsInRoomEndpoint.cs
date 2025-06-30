using FastEndpoints;
using HostelManagementSystemApi.Features.Beds.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Beds
{
    public class GetBedsInRoomEndpoint : EndpointWithoutRequest<List<BedResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetBedsInRoomEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/rooms/{RoomID}/beds");
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

            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserID == int.Parse(userId), ct);
            if (vendor == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var roomID = Route<int>("RoomID");
            var room = await _context.Rooms
                .Include(r => r.Hostel)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoomID == roomID, ct);

            if (room == null || room.Hostel == null || room.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var beds = await _context.Beds
                .Where(b => b.RoomID == roomID)
                .Include(b => b.Student)
                .ThenInclude(s => s!.User)
                .AsNoTracking()
                .Select(b => new BedResponse
                {
                    BedID = b.BedID,
                    BedNumber = b.BedNumber,
                    RoomID = b.RoomID,
                    StudentID = b.StudentID,
                    StudentName = b.Student != null && b.Student.User != null ? b.Student.User.Name : null
                })
                .ToListAsync(ct);

            await SendAsync(beds, 200, ct);
        }
    }
}
