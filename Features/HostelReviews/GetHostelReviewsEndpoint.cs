using FastEndpoints;
using HostelManagementSystemApi.Features.HostelReviews.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.HostelReviews
{
    public class GetHostelReviewsEndpoint : EndpointWithoutRequest<List<HostelReviewResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetHostelReviewsEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/hostels/{HostelID}/reviews");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var hostelId = Route<int>("HostelID");

            var hostelExists = await _context.Hostels.AnyAsync(h => h.HostelID == hostelId, ct);
            if (!hostelExists)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var reviews = await _context.HostelReviews
                .Where(r => r.HostelID == hostelId)
                .Include(r => r.Student)!.ThenInclude(s => s!.User)
                .Include(r => r.Hostel)
                .AsNoTracking()
                .Select(r => new HostelReviewResponse
                {
                    HostelReviewId = r.HostelReviewId,
                    HostelID = r.HostelID,
                    HostelName = r.Hostel!.Name,
                    StudentID = r.StudentID,
                    StudentName = r.Student!.User!.Name,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Date = r.Date
                })
                .ToListAsync(ct);

            await SendAsync(reviews, 200, ct);
        }
    }
}
