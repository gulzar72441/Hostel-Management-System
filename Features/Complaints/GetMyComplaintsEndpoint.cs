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
    public class GetMyComplaintsEndpoint : Endpoint<GetMyComplaintsRequest, PagedResponse<ComplaintResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetMyComplaintsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/complaints/my-history");
            Roles("Student");
        }

        public override async Task HandleAsync(GetMyComplaintsRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserID == int.Parse(userId), ct);

            if (student == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var query = _context.Complaints
                .Where(c => c.StudentID == student.StudentID)
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
