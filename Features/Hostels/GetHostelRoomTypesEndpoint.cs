using FastEndpoints;
using HostelManagementSystemApi.Features.Hostels.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class GetHostelRoomTypesEndpoint : EndpointWithoutRequest<List<RoomTypeResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetHostelRoomTypesEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/hostels/{HostelID}/room-types");
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

            var hostelId = Route<int>("HostelID");

            var hostel = await _context.Hostels.AsNoTracking()
                .FirstOrDefaultAsync(h => h.HostelID == hostelId && h.VendorID == vendor.VendorID, ct);

            if (hostel == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var roomTypes = await _context.RoomTypes.AsNoTracking()
                .Where(rt => rt.HostelID == hostelId)
                .Select(rt => new RoomTypeResponse
                {
                    RoomTypeID = rt.RoomTypeID,
                    Name = rt.Name,
                    Description = rt.Description,
                    Price = rt.Price,
                    Capacity = rt.Capacity,
                    IsActive = rt.IsActive
                })
                .ToListAsync(ct);

            await SendAsync(roomTypes, 200, ct);
        }
    }
}
