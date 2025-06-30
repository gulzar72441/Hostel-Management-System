using FastEndpoints;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Features.Reviews.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Reviews
{
    public class UpdateReviewEndpoint : Endpoint<UpdateReviewRequest, HostelReviewResponse>
    {
        private readonly ApplicationDbContext _context;

        public UpdateReviewEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/reviews/{HostelReviewId}");
            Roles("Student");
        }

        public override async Task HandleAsync(UpdateReviewRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var student = await _context.Students.AsNoTracking().Include(s => s.User).FirstOrDefaultAsync(s => s.UserID == int.Parse(userId), ct);
            if (student == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var hostelReviewId = Route<int>("HostelReviewId");
            var review = await _context.HostelReviews.Include(r => r.Hostel).FirstOrDefaultAsync(r => r.HostelReviewId == hostelReviewId, ct);

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

            review.Rating = req.Rating;
            review.Comment = req.Comment;
            review.Date = DateTime.UtcNow; // Update the date to reflect the edit time

            await _context.SaveChangesAsync(ct);

            var response = new HostelReviewResponse
            {
                HostelReviewId = review.HostelReviewId,
                HostelID = review.HostelID,
                StudentID = review.StudentID,
                StudentName = student.User?.Name ?? string.Empty,
                Rating = review.Rating,
                Comment = review.Comment,
                Date = review.Date
            };

            await SendAsync(response, 200, ct);
        }
    }
}
