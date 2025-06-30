using FastEndpoints;
using HostelManagementSystemApi.Features.Beds.DTOs;
using HostelManagementSystemApi.Features.Common;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Beds
{
    public class GetHostelBedsEndpoint : Endpoint<GetHostelBedsRequest, PagedResponse<BedResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetHostelBedsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/hostels/{HostelID}/beds");
            Roles("Vendor");
        }

        public override async Task HandleAsync(GetHostelBedsRequest req, CancellationToken ct)
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

            var query = _context.Beds
                .Where(b => b.Room != null && b.Room.HostelID == hostelId)
                .AsNoTracking();

            // Apply filtering
            if (req.IsOccupied.HasValue)
            {
                query = query.Where(b => b.IsOccupied == req.IsOccupied.Value);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(req.SortBy))
            {
                var isDescending = string.Equals(req.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
                switch (req.SortBy.ToLower())
                {
                    case "bednumber":
                        query = isDescending ? query.OrderByDescending(b => b.BedNumber) : query.OrderBy(b => b.BedNumber);
                        break;
                    default:
                        query = query.OrderBy(b => b.BedNumber);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(b => b.BedNumber);
            }

            var totalCount = await query.CountAsync(ct);

            var beds = await query
                .Include(b => b.Room)
                .Include(b => b.Student)!.ThenInclude(s => s!.User)
                .Select(b => new BedResponse
                {
                    BedID = b.BedID,
                    BedNumber = b.BedNumber,
                    RoomID = b.RoomID,
                    RoomNumber = b.Room != null ? b.Room.RoomNumber : string.Empty,
                    IsOccupied = b.IsOccupied,
                    StudentID = b.StudentID,
                    StudentName = b.Student != null && b.Student.User != null ? b.Student.User.Name : null
                })
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync(ct);

            var response = new PagedResponse<BedResponse>(beds, req.PageNumber, req.PageSize, totalCount);

            await SendAsync(response, 200, ct);
        }
    }
}
