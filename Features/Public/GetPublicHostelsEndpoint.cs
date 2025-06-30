using FastEndpoints;
using FluentValidation;
using HostelManagementSystemApi.Features.Hostels.DTOs.Public;
using HostelManagementSystemApi.Persistence;
using HostelManagementSystemApi.Shared;
using Microsoft.EntityFrameworkCore;

namespace HostelManagementSystemApi.Features.Public
{
    public class GetPublicHostelsRequest
    {
        [FromQuery] public string? SearchTerm { get; set; }
        [FromQuery] public string? City { get; set; }
        [FromQuery] public string? State { get; set; }
        [FromQuery] public string? Country { get; set; }
        [FromQuery] public int Page { get; set; } = 1;
        [FromQuery] public int PageSize { get; set; } = 10;
        [FromQuery] public string? SortBy { get; set; }
        [FromQuery] public string? SortOrder { get; set; }
    }

    public class GetPublicHostelsValidator : Validator<GetPublicHostelsRequest>
    {
        public GetPublicHostelsValidator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0);
            RuleFor(x => x.SortBy).Must(x => x == null || new[] { "name", "city" }.Contains(x.ToLower()))
                .WithMessage("SortBy must be 'name' or 'city'.");
            RuleFor(x => x.SortOrder).Must(x => x == null || new[] { "asc", "desc" }.Contains(x.ToLower()))
                .WithMessage("SortOrder must be 'asc' or 'desc'.");
        }
    }

    public class GetPublicHostelsEndpoint : Endpoint<GetPublicHostelsRequest, PaginatedResponse<PublicHostelResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetPublicHostelsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/public/hostels");
            AllowAnonymous();
            Validator<GetPublicHostelsValidator>();
            ResponseCache(60); // Cache for 60 seconds
        }

        public override async Task HandleAsync(GetPublicHostelsRequest req, CancellationToken ct)
        {
            var query = _context.Hostels.Where(h => h.IsApproved).AsQueryable();

            if (!string.IsNullOrEmpty(req.SearchTerm))
            {
                query = query.Where(h => h.Name.Contains(req.SearchTerm) || (h.Description != null && h.Description.Contains(req.SearchTerm)));
            }

            if (!string.IsNullOrEmpty(req.City))
            {
                query = query.Where(h => h.City.Equals(req.City, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(req.State))
            {
                query = query.Where(h => h.State.Equals(req.State, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(req.Country))
            {
                query = query.Where(h => h.Country.Equals(req.Country, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(req.SortBy))
            {
                var isDescending = req.SortOrder?.ToLower() == "desc";
                query = req.SortBy.ToLower() switch
                {
                    "name" => isDescending ? query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name),
                    "city" => isDescending ? query.OrderByDescending(h => h.City) : query.OrderBy(h => h.City),
                    _ => query.OrderBy(h => h.Name)
                };
            }
            else
            {
                query = query.OrderBy(h => h.Name);
            }

            var totalCount = await query.CountAsync(ct);

            var approvedHostels = await query
                .Select(h => new PublicHostelResponse
                {
                    HostelID = h.HostelID,
                    Name = h.Name,
                    Address = h.Address,
                    City = h.City,
                    State = h.State,
                    Country = h.Country,
                    Description = h.Description,
                    ContactPerson = h.ContactPerson,
                    ContactEmail = h.ContactEmail,
                    ContactPhone = h.ContactPhone
                })
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync(ct);

            var response = new PaginatedResponse<PublicHostelResponse>
            {
                Items = approvedHostels,
                Page = req.Page,
                PageSize = req.PageSize,
                TotalCount = totalCount
            };

            await SendAsync(response, 200, ct);
        }
    }
}
