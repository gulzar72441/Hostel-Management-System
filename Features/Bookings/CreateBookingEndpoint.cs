using FastEndpoints;
using HostelManagementSystemApi.Features.Bookings.DTOs;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Bookings
{
    public class CreateBookingEndpoint : Endpoint<CreateBookingRequest, BookingResponse>
    {
        private readonly ApplicationDbContext _context;

        public CreateBookingEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/bookings");
            Roles("Student");
        }

        public override async Task HandleAsync(CreateBookingRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.UserID == int.Parse(userId), ct);
            if (student == null || student.User == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var room = await _context.Rooms
                .Include(r => r.Hostel)
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RoomID == req.RoomID, ct);

            if (room == null || room.Hostel == null || !room.Hostel.IsApproved || !room.IsAvailable || room.RoomType == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (req.CheckInDate >= req.CheckOutDate)
            {
                AddError("Check-out date must be after check-in date.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var overlappingBooking = await _context.Bookings
                .AnyAsync(b => b.RoomID == req.RoomID &&
                               b.CheckInDate < req.CheckOutDate &&
                               b.CheckOutDate > req.CheckInDate, ct);

            if (overlappingBooking)
            {
                AddError("The room is already booked for the selected dates.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var booking = new Booking
            {
                StudentID = student.StudentID,
                RoomID = req.RoomID,
                BookingDate = DateTime.UtcNow,
                CheckInDate = req.CheckInDate,
                CheckOutDate = req.CheckOutDate,
                Status = "Confirmed",
                TotalPrice = room.RoomType.Price * (decimal)(req.CheckOutDate - req.CheckInDate).TotalDays
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(ct);

            var response = new BookingResponse
            {
                BookingID = booking.BookingID,
                StudentID = student.StudentID,
                StudentName = student.User.Name,
                HostelID = room.HostelID,
                HostelName = room.Hostel.Name,
                RoomID = room.RoomID,
                RoomNumber = room.RoomNumber,
                BookingDate = booking.BookingDate,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                Status = booking.Status,
                TotalPrice = booking.TotalPrice
            };

            await SendAsync(response, 201, ct);
        }
    }
}
