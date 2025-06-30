using FastEndpoints;
using HostelManagementSystemApi.Features.Rooms.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HostelManagementSystemApi.Features.Rooms
{
    public class GetRoomByIdRequest
    {
        public int RoomID { get; set; }
    }

    public class GetRoomByIdEndpoint : Endpoint<GetRoomByIdRequest, RoomResponse>
    {
        private readonly ApplicationDbContext _context;

        public GetRoomByIdEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/public/rooms/{RoomID}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetRoomByIdRequest req, CancellationToken ct)
        {
            var room = await _context.Rooms
                .AsNoTracking()
                .Include(r => r.Hostel)
                .Include(r => r.RoomType)
                .Where(r => r.RoomID == req.RoomID && r.Hostel != null && r.Hostel.IsApproved && r.RoomType != null)
                .Select(r => new RoomResponse
                {
                    RoomID = r.RoomID,
                    RoomNumber = r.RoomNumber,
                    Description = r.Description,
                    IsAvailable = r.IsAvailable,
                    RoomTypeID = r.RoomType!.RoomTypeID,
                    RoomTypeName = r.RoomType.Name,
                    RoomTypePrice = r.RoomType.Price,
                    RoomTypeCapacity = r.RoomType.Capacity
                })
                .FirstOrDefaultAsync(ct);

            if (room == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendAsync(room, 200, ct);
        }
    }
}
