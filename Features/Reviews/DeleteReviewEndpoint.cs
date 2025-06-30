using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Reviews
{
    public class DeleteReviewEndpoint : EndpointWithoutRequest
    {
        private readonly ApplicationDbContext _context;

        public DeleteReviewEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/reviews/{HostelReviewId}");
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

            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.UserID == int.Parse(userId), ct);
            if (student == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var hostelReviewId = Route<int>("HostelReviewId");
            var review = await _context.HostelReviews.FirstOrDefaultAsync(r => r.HostelReviewId == hostelReviewId, ct);

            if (review == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (review.StudentID != student.StudentID)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            _context.HostelReviews.Remove(review);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
