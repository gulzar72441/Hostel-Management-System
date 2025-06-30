using FastEndpoints;
using HostelManagementSystemApi.Features.Common;
using HostelManagementSystemApi.Features.Reviews.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Reviews
{
    public class GetHostelReviewsEndpoint : Endpoint<GetHostelReviewsRequest, PagedResponse<HostelReviewResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetHostelReviewsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/public/hostels/{HostelID}/reviews");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetHostelReviewsRequest req, CancellationToken ct)
        {
            var hostelId = Route<int>("HostelID");

            var query = _context.HostelReviews
                .Where(r => r.HostelID == hostelId)
                .AsNoTracking();

            if (req.Rating.HasValue)
            {
                query = query.Where(r => r.Rating == req.Rating.Value);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(req.SortBy))
            {
                var isDescending = string.Equals(req.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
                switch (req.SortBy.ToLower())
                {
                    case "rating":
                        query = isDescending ? query.OrderByDescending(r => r.Rating) : query.OrderBy(r => r.Rating);
                        break;
                    case "date":
                        query = isDescending ? query.OrderByDescending(r => r.Date) : query.OrderBy(r => r.Date);
                        break;
                    default:
                        query = query.OrderByDescending(r => r.Date);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(r => r.Date);
            }

            var totalCount = await query.CountAsync(ct);

            var reviews = await query
                .Include(r => r.Student)
                .ThenInclude(s => s!.User)
                .Select(r => new HostelReviewResponse
                {
                    HostelReviewId = r.HostelReviewId,
                    HostelID = r.HostelID,
                    StudentID = r.StudentID,
                    StudentName = r.Student!.User!.Name,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Date = r.Date
                })
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync(ct);

            var response = new PagedResponse<HostelReviewResponse>(reviews, req.PageNumber, req.PageSize, totalCount);

            await SendAsync(response, 200, ct);
        }
    }
}
