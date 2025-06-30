using FastEndpoints;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Features.Reviews.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Reviews
{
    public class CreateReviewEndpoint : Endpoint<CreateReviewRequest, HostelReviewResponse>
    {
        private readonly ApplicationDbContext _context;

        public CreateReviewEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/reviews");
            Roles("Student");
        }

        public override async Task HandleAsync(CreateReviewRequest req, CancellationToken ct)
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

            // Validate that the student has a booking for this hostel
            var hasBooking = await _context.Bookings
                .Include(b => b.Room)
                .AnyAsync(b => b.StudentID == student.StudentID && b.Room != null && b.Room.HostelID == req.HostelID && b.Status == "Confirmed", ct);

            if (!hasBooking)
            {
                AddError("You can only review hostels you have a confirmed booking with.");
                await SendErrorsAsync(403, ct);
                return;
            }
            
            // Prevent duplicate reviews
            var existingReview = await _context.HostelReviews.AnyAsync(r => r.StudentID == student.StudentID && r.HostelID == req.HostelID, ct);
            if (existingReview)
            {
                AddError("You have already submitted a review for this hostel.");
                await SendErrorsAsync(409, ct);
                return;
            }

            var review = new HostelReview
            {
                HostelID = req.HostelID,
                StudentID = student.StudentID,
                Rating = req.Rating,
                Comment = req.Comment,
                Date = DateTime.UtcNow
            };

            _context.HostelReviews.Add(review);
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

            await SendAsync(response, 201, ct);
        }
    }
}
