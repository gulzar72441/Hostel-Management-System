using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.HostelReviews
{
    public class DeleteHostelReviewEndpoint : EndpointWithoutRequest
    {
        private readonly ApplicationDbContext _context;

        public DeleteHostelReviewEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/hostel-reviews/{HostelReviewId}");
            Roles("Student");
        }

        public override async Task HandleAsync(CancellationToken ct)
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

            var reviewId = Route<int>("HostelReviewId");
            var review = await _context.HostelReviews.FirstOrDefaultAsync(r => r.HostelReviewId == reviewId, ct);

            if (review == null || review.StudentID != student.StudentID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            _context.HostelReviews.Remove(review);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
