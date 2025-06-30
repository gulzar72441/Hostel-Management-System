using FastEndpoints;
using FluentValidation;
using HostelManagementSystemApi.Features.Rooms.DTOs;
using HostelManagementSystemApi.Persistence;
using HostelManagementSystemApi.Shared;
using Microsoft.EntityFrameworkCore;

namespace HostelManagementSystemApi.Features.Rooms
{
    public class GetHostelRoomsRequest
    {
        public int HostelID { get; set; }
        [FromQuery] public int Page { get; set; } = 1;
        [FromQuery] public int PageSize { get; set; } = 10;
        [FromQuery] public string? SortBy { get; set; }
        [FromQuery] public string? SortOrder { get; set; }
        [FromQuery] public string? RoomTypeName { get; set; }
        [FromQuery] public int? Capacity { get; set; }
        [FromQuery] public decimal? MaxPrice { get; set; }
    }

    public class GetHostelRoomsValidator : Validator<GetHostelRoomsRequest>
    {
        public GetHostelRoomsValidator()
        {
            RuleFor(x => x.HostelID).GreaterThan(0);
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0);

            RuleFor(x => x.SortBy).Must(x => x == null || new[] { "price", "type" }.Contains(x.ToLower()))
                .WithMessage("SortBy must be 'price' or 'type'.");

            RuleFor(x => x.SortOrder).Must(x => x == null || new[] { "asc", "desc" }.Contains(x.ToLower()))
                .WithMessage("SortOrder must be 'asc' or 'desc'.");

            RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
            RuleFor(x => x.MaxPrice).GreaterThan(0).When(x => x.MaxPrice.HasValue);
        }
    }

    public class GetHostelRoomsEndpoint : Endpoint<GetHostelRoomsRequest, PaginatedResponse<RoomResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetHostelRoomsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/public/hostels/{HostelID}/rooms");
            AllowAnonymous();
            Validator<GetHostelRoomsValidator>();
            ResponseCache(60); // Cache for 60 seconds
        }

        public override async Task HandleAsync(GetHostelRoomsRequest req, CancellationToken ct)
        {
            var hostelExistsAndIsApproved = await _context.Hostels
                .AnyAsync(h => h.HostelID == req.HostelID && h.IsApproved, ct);

            if (!hostelExistsAndIsApproved)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var query = _context.Rooms
                .AsNoTracking()
                .Include(r => r.RoomType)
                .Where(r => r.HostelID == req.HostelID && r.RoomType != null);

            if (!string.IsNullOrEmpty(req.RoomTypeName))
            {
                query = query.Where(r => r.RoomType!.Name.Contains(req.RoomTypeName));
            }

            if (req.Capacity.HasValue)
            {
                query = query.Where(r => r.RoomType!.Capacity >= req.Capacity.Value);
            }

            if (req.MaxPrice.HasValue)
            {
                query = query.Where(r => r.RoomType!.Price <= req.MaxPrice.Value);
            }

            if (!string.IsNullOrEmpty(req.SortBy))
            {
                var isDescending = req.SortOrder?.ToLower() == "desc";
                query = req.SortBy.ToLower() switch
                {
                    "price" => isDescending ? query.OrderByDescending(r => r.RoomType!.Price) : query.OrderBy(r => r.RoomType!.Price),
                    "type" => isDescending ? query.OrderByDescending(r => r.RoomType!.Name) : query.OrderBy(r => r.RoomType!.Name),
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
                    RoomTypeName = r.RoomType.Name,
                    RoomTypePrice = r.RoomType.Price,
                    RoomTypeCapacity = r.RoomType.Capacity
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
