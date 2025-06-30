using FastEndpoints;
using HostelManagementSystemApi.Features.Bookings.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Bookings
{
    public class CancelBookingEndpoint : EndpointWithoutRequest<BookingResponse>
    {
        private readonly ApplicationDbContext _context;

        public CancelBookingEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/bookings/{BookingID}/cancel");
            Roles("Student");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var student = await _context.Students
                .Include(s => s.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserID == int.Parse(userId), ct);

            if (student == null || student.User == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var bookingId = Route<int>("BookingID");
            var booking = await _context.Bookings
                .Include(b => b.Room)!.ThenInclude(r => r!.Hostel)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.StudentID == student.StudentID, ct);

            if (booking == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (booking.Status != "Confirmed" && booking.Status != "Pending")
            {
                AddError("This booking cannot be cancelled as it is already processed or completed.");
                await SendErrorsAsync(400, ct);
                return;
            }

            if (booking.CheckInDate <= DateTime.UtcNow)
            {
                AddError("Cannot cancel a booking that has already started or is in the past.");
                await SendErrorsAsync(400, ct);
                return;
            }

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync(ct);

            var response = new BookingResponse
            {
                BookingID = booking.BookingID,
                StudentID = booking.StudentID,
                StudentName = student.User.Name,
                HostelID = booking.Room!.HostelID,
                HostelName = booking.Room.Hostel!.Name,
                RoomID = booking.RoomID,
                RoomNumber = booking.Room.RoomNumber,
                BookingDate = booking.BookingDate,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                Status = booking.Status,
                TotalPrice = booking.TotalPrice
            };

            await SendAsync(response, 200, ct);
        }
    }
}
