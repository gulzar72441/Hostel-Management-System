using FastEndpoints;
using HostelManagementSystemApi.Features.Bookings.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HostelManagementSystemApi.Features.Bookings
{
    public class GetAllBookingsEndpoint : EndpointWithoutRequest<List<BookingResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllBookingsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/admin/bookings");
            Roles("Admin");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var bookings = await _context.Bookings
                .Select(b => new BookingResponse
                {
                    BookingID = b.BookingID,
                    RoomID = b.RoomID,
                    StudentID = b.StudentID,
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
