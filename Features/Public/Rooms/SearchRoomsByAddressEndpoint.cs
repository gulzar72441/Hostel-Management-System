using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using HostelManagementSystemApi.Features.Rooms.DTOs.Public;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HostelManagementSystemApi.Features.Public.Rooms
{
    public class SearchRoomsByAddressEndpoint : Endpoint<SearchRoomsByAddressRequest, List<RoomResponse>>
    {
        private readonly ApplicationDbContext _context;

        public SearchRoomsByAddressEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/public/rooms/search");
            AllowAnonymous();
        }

        public override async Task HandleAsync(SearchRoomsByAddressRequest req, CancellationToken ct)
        {
            var rooms = await _context.Rooms
                .Include(r => r.Hostel)
                .Include(r => r.RoomType)
                .Where(r => r.Hostel.Address.Contains(req.Address) || r.Hostel.City.Contains(req.Address) || r.Hostel.State.Contains(req.Address))
                .Select(r => new RoomResponse
                {
                    RoomID = r.RoomID,
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType.Name,
                    Description = r.Description,
                    Capacity = r.RoomType.Capacity,
                    Price = r.RoomType.Price,
                    IsAvailable = r.IsAvailable
                })
                .ToListAsync(ct);

            await SendAsync(rooms, 200, ct);
        }
    }
}

