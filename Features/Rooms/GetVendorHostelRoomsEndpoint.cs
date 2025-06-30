using FastEndpoints;
using FluentValidation;
using HostelManagementSystemApi.Features.Rooms.DTOs;
using HostelManagementSystemApi.Persistence;
using HostelManagementSystemApi.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Rooms
{
    public class GetVendorHostelRoomsValidator : Validator<GetHostelRoomsRequest>
    {
        public GetVendorHostelRoomsValidator()
        {
            RuleFor(x => x.HostelID).GreaterThan(0);
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0);
            RuleFor(x => x.SortBy).Must(x => x == null || new[] { "number", "price", "capacity" }.Contains(x.ToLower()))
                .WithMessage("SortBy must be 'number', 'price', or 'capacity'.");
            RuleFor(x => x.SortOrder).Must(x => x == null || new[] { "asc", "desc" }.Contains(x.ToLower()))
                .WithMessage("SortOrder must be 'asc' or 'desc'.");
        }
    }

    public class GetVendorHostelRoomsEndpoint : Endpoint<GetHostelRoomsRequest, PaginatedResponse<RoomResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetVendorHostelRoomsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/rooms/hostel/{HostelID}");
            Roles("Vendor");
            Validator<GetVendorHostelRoomsValidator>();
        }

        public override async Task HandleAsync(GetHostelRoomsRequest req, CancellationToken ct)
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

            var hostel = await _context.Hostels.FirstOrDefaultAsync(h => h.HostelID == req.HostelID && h.VendorID == vendor.VendorID, ct);
            if (hostel == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var query = _context.Rooms.AsNoTracking()
                .Include(r => r.RoomType)
                .Where(r => r.HostelID == req.HostelID && r.RoomType != null);

            if (!string.IsNullOrEmpty(req.SortBy))
            {
                var isDescending = req.SortOrder?.ToLower() == "desc";
                query = req.SortBy.ToLower() switch
                {
                    "number" => isDescending ? query.OrderByDescending(r => r.RoomNumber) : query.OrderBy(r => r.RoomNumber),
                    "price" => isDescending ? query.OrderByDescending(r => r.RoomType!.Price) : query.OrderBy(r => r.RoomType!.Price),
                    "capacity" => isDescending ? query.OrderByDescending(r => r.RoomType!.Capacity) : query.OrderBy(r => r.RoomType!.Capacity),
                    _ => query.OrderBy(r => r.RoomNumber)
                };
            }
            else
            {
                query = query.OrderBy(r => r.RoomNumber);
            }

            var totalCount = await query.CountAsync(ct);

            var rooms = await query
                .Select(r => new RoomResponse
                {
                    RoomID = r.RoomID,
                    RoomNumber = r.RoomNumber,
                    Description = r.Description,
                    IsAvailable = r.IsAvailable,
                    RoomTypeID = r.RoomType!.RoomTypeID,
                    RoomTypeName = r.RoomType!.Name,
                    RoomTypePrice = r.RoomType!.Price,
                    RoomTypeCapacity = r.RoomType!.Capacity
                })
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync(ct);

            var response = new PaginatedResponse<RoomResponse>
            {
                Items = rooms,
                Page = req.Page,
                PageSize = req.PageSize,
                TotalCount = totalCount
            };

            await SendAsync(response, 200, ct);
        }
    }
}
