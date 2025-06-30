using FastEndpoints;
using HostelManagementSystemApi.Features.Bookings.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Bookings
{
    public class GetHostelBookingsEndpoint : EndpointWithoutRequest<List<BookingResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetHostelBookingsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/hostels/{HostelID}/bookings");
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

            var vendor = await _context.Vendors.AsNoTracking().FirstOrDefaultAsync(v => v.UserID == int.Parse(userId), ct);
            if (vendor == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var hostelId = Route<int>("HostelID");
            var hostel = await _context.Hostels.AsNoTracking().FirstOrDefaultAsync(h => h.HostelID == hostelId, ct);

            if (hostel == null || hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var bookings = await _context.Bookings
                .Where(b => b.Room!.HostelID == hostelId)
                .Include(b => b.Student)!.ThenInclude(s => s!.User)
                .Include(b => b.Room)!.ThenInclude(r => r!.Hostel)
                .AsNoTracking()
                .Select(b => new BookingResponse
                {
                    BookingID = b.BookingID,
                    StudentID = b.StudentID,
                    StudentName = b.Student!.User!.Name,
                    HostelID = b.Room!.HostelID,
                    HostelName = b.Room.Hostel!.Name,
                    RoomID = b.RoomID,
                    RoomNumber = b.Room.RoomNumber,
                    BookingDate = b.BookingDate,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Status = b.Status,
                    TotalPrice = b.TotalPrice
                })
                .ToListAsync(ct);

            await SendAsync(bookings, 200, ct);
        }
    }
}
