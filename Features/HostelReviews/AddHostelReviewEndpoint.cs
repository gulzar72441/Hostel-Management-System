using FastEndpoints;
using HostelManagementSystemApi.Features.HostelReviews.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HostelManagementSystemApi.Domain;
using System;

namespace HostelManagementSystemApi.Features.HostelReviews
{
    public class AddHostelReviewEndpoint : Endpoint<AddHostelReviewRequest, HostelReviewResponse>
    {
        private readonly ApplicationDbContext _context;

        public AddHostelReviewEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/hostel-reviews");
            Roles("Student");
        }

        public override async Task HandleAsync(AddHostelReviewRequest req, CancellationToken ct)
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

            var hostel = await _context.Hostels.AsNoTracking().FirstOrDefaultAsync(h => h.HostelID == req.HostelID, ct);
            if (hostel == null)
            {
                AddError("Hostel not found.");
                await SendErrorsAsync(404, ct);
                return;
            }

            var reviewExists = await _context.HostelReviews
                .AnyAsync(r => r.HostelID == req.HostelID && r.StudentID == student.StudentID, ct);

            if (reviewExists)
            {
                AddError("You have already reviewed this hostel.");
                await SendErrorsAsync(409, ct);
                return;
            }

            var newReview = new HostelReview
            {
                HostelID = req.HostelID,
                StudentID = student.StudentID,
                Rating = req.Rating,
                Comment = req.Comment,
                Date = DateTime.UtcNow
            };

            _context.HostelReviews.Add(newReview);
            await _context.SaveChangesAsync(ct);

            var response = new HostelReviewResponse
            {
                HostelReviewId = newReview.HostelReviewId,
                HostelID = newReview.HostelID,
                HostelName = hostel.Name,
                StudentID = newReview.StudentID,
                StudentName = student.User!.Name,
                Rating = newReview.Rating,
                Comment = newReview.Comment,
                Date = newReview.Date
            };

            await SendAsync(response, 201, ct);
        }
    }
}
