using FastEndpoints;
using HostelManagementSystemApi.Features.HostelReviews.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;

namespace HostelManagementSystemApi.Features.HostelReviews
{
    public class UpdateHostelReviewEndpoint : Endpoint<UpdateHostelReviewRequest, HostelReviewResponse>
    {
        private readonly ApplicationDbContext _context;

        public UpdateHostelReviewEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/hostel-reviews/{HostelReviewId}");
            Roles("Student");
        }

        public override async Task HandleAsync(UpdateHostelReviewRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var student = await _context.Students
                .Include(s => s.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserID == int.Parse(userId), ct);

            if (student == null)
            {
                AddError("Student profile not found.");
                await SendErrorsAsync(404, ct);
                return;
            }

            var reviewId = Route<int>("HostelReviewId");
            var review = await _context.HostelReviews
                .Include(r => r.Hostel)
                .FirstOrDefaultAsync(r => r.HostelReviewId == reviewId, ct);

            if (review == null || review.StudentID != student.StudentID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            review.Rating = req.Rating;
            review.Comment = req.Comment;
            review.Date = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            var response = new HostelReviewResponse
            {
                HostelReviewId = review.HostelReviewId,
                HostelID = review.HostelID,
                HostelName = review.Hostel!.Name,
                StudentID = review.StudentID,
                StudentName = student.User!.Name,
                Rating = review.Rating,
                Comment = review.Comment,
                Date = review.Date
            };

            await SendAsync(response, 200, ct);
        }
    }
}
