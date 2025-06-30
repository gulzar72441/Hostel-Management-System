using FastEndpoints;
using FluentValidation;
using HostelManagementSystemApi.Features.Bookings.DTOs;
using HostelManagementSystemApi.Persistence;
using HostelManagementSystemApi.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Bookings
{
    public class GetVendorBookingsRequest
    {
        [FromQuery] public int Page { get; set; } = 1;
        [FromQuery] public int PageSize { get; set; } = 10;
        [FromQuery] public string? SortBy { get; set; }
        [FromQuery] public string? SortOrder { get; set; }
    }

    public class GetVendorBookingsValidator : Validator<GetVendorBookingsRequest>
    {
        public GetVendorBookingsValidator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0);
            RuleFor(x => x.SortBy).Must(x => x == null || new[] { "date", "status" }.Contains(x.ToLower()))
                .WithMessage("SortBy must be 'date' or 'status'.");
            RuleFor(x => x.SortOrder).Must(x => x == null || new[] { "asc", "desc" }.Contains(x.ToLower()))
                .WithMessage("SortOrder must be 'asc' or 'desc'.");
        }
    }

    public class GetVendorBookingsEndpoint : Endpoint<GetVendorBookingsRequest, PaginatedResponse<BookingResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetVendorBookingsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/bookings/vendor-bookings");
            Roles("Vendor");
            Validator<GetVendorBookingsValidator>();
        }

        public override async Task HandleAsync(GetVendorBookingsRequest req, CancellationToken ct)
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

            var hostelIds = await _context.Hostels
                .Where(h => h.VendorID == vendor.VendorID)
                .Select(h => h.HostelID)
                .ToListAsync(ct);

            var query = _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.Room != null && hostelIds.Contains(b.Room.HostelID));

            if (!string.IsNullOrEmpty(req.SortBy))
            {
                var isDescending = req.SortOrder?.ToLower() == "desc";
                query = req.SortBy.ToLower() switch
                {
                    "date" => isDescending ? query.OrderByDescending(b => b.CheckInDate) : query.OrderBy(b => b.CheckInDate),
                    "status" => isDescending ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status),
                    _ => query.OrderByDescending(b => b.BookingDate)
                };
            }
            else
            {
                query = query.OrderByDescending(b => b.BookingDate);
            }

            var totalCount = await query.CountAsync(ct);

            var bookings = await query
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
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync(ct);

            var response = new PaginatedResponse<BookingResponse>
            {
                Items = bookings,
                Page = req.Page,
                PageSize = req.PageSize,
                TotalCount = totalCount
            };

            await SendAsync(response, 200, ct);
        }
    }
}
