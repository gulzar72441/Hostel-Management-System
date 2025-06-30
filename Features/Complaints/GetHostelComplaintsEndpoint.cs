using FastEndpoints;
using HostelManagementSystemApi.Features.Common;
using HostelManagementSystemApi.Features.Complaints.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Complaints
{
    public class GetHostelComplaintsEndpoint : Endpoint<GetHostelComplaintsRequest, PagedResponse<ComplaintResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetHostelComplaintsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/hostels/{HostelID}/complaints");
            Roles("Vendor");
        }

        public override async Task HandleAsync(GetHostelComplaintsRequest req, CancellationToken ct)
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

            var query = _context.Complaints
                .Where(c => c.HostelID == hostelId)
                .AsNoTracking();

            // Apply filtering
            if (!string.IsNullOrEmpty(req.Status))
            {
                query = query.Where(c => c.Status == req.Status);
            }

            if (!string.IsNullOrEmpty(req.Type))
            {
                query = query.Where(c => c.Type == req.Type);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(req.SortBy))
            {
                var isDescending = string.Equals(req.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
                switch (req.SortBy.ToLower())
                {
                    case "date":
                        query = isDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt);
                        break;
                    default:
                        query = query.OrderByDescending(c => c.CreatedAt);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(c => c.CreatedAt);
            }

            var totalCount = await query.CountAsync(ct);

            var complaints = await query
                .Include(c => c.Student)!.ThenInclude(s => s!.User)
                .Include(c => c.Hostel)
                .Select(c => new ComplaintResponse
                {
                    ComplaintID = c.ComplaintID,
                    StudentID = c.StudentID,
                    StudentName = c.Student!.User!.Name,
                    HostelID = c.HostelID,
                    HostelName = c.Hostel!.Name,
                    Type = c.Type,
                    Description = c.Description,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,
                    ResolvedAt = c.ResolvedAt
                })
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync(ct);

            var response = new PagedResponse<ComplaintResponse>(complaints, req.PageNumber, req.PageSize, totalCount);

            await SendAsync(response, 200, ct);
        }
    }
}
