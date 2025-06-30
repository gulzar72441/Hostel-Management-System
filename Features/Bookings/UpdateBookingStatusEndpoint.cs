using FastEndpoints;
using HostelManagementSystemApi.Features.Bookings.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Bookings
{
        public class UpdateBookingStatusEndpoint : Endpoint<UpdateBookingStatusRequest, BookingResponse>
    {
        private readonly ApplicationDbContext _context;

        public UpdateBookingStatusEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/bookings/{BookingID}/status");
            Roles("Vendor");
        }

        public override async Task HandleAsync(UpdateBookingStatusRequest req, CancellationToken ct)
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

            var bookingId = Route<int>("BookingID");
            var booking = await _context.Bookings
                .Include(b => b.Room)!.ThenInclude(r => r!.Hostel)
                .Include(b => b.Student)!.ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId, ct);

            if (booking == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (booking.Room == null || booking.Room.Hostel == null || booking.Room.Hostel.VendorID != vendor.VendorID)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var allowedStatuses = new[] { "Confirmed", "Checked-in", "Completed", "No-show", "Cancelled" };
            if (!allowedStatuses.Contains(req.Status))
            {
                AddError($"Invalid status. Allowed statuses are: {string.Join(", ", allowedStatuses)}");
                await SendErrorsAsync(400, ct);
                return;
            }

            booking.Status = req.Status;
            await _context.SaveChangesAsync(ct);

            var response = new BookingResponse
            {
                BookingID = booking.BookingID,
                StudentID = booking.StudentID,
                StudentName = booking.Student!.User!.Name,
                HostelID = booking.Room.HostelID,
                HostelName = booking.Room.Hostel.Name,
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
