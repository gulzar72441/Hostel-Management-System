using FastEndpoints;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Features.Complaints.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Complaints
{
    public class CreateComplaintEndpoint : Endpoint<CreateComplaintRequest, ComplaintResponse>
    {
        private readonly ApplicationDbContext _context;

        public CreateComplaintEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/complaints");
            Roles("Student");
        }

        public override async Task HandleAsync(CreateComplaintRequest req, CancellationToken ct)
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

            // Validate that the student belongs to the hostel they are complaining about
            if (student.HostelID != req.HostelID)
            {
                AddError("You can only submit complaints for the hostel you are residing in.");
                await SendErrorsAsync(403, ct);
                return;
            }

            var complaint = new Complaint
            {
                StudentID = student.StudentID,
                HostelID = req.HostelID,
                Type = req.Type,
                Description = req.Description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync(ct);

            var hostel = await _context.Hostels.FindAsync(req.HostelID);

            var response = new ComplaintResponse
            {
                ComplaintID = complaint.ComplaintID,
                StudentID = complaint.StudentID,
                StudentName = student.User?.Name ?? string.Empty,
                HostelID = complaint.HostelID,
                HostelName = hostel?.Name ?? string.Empty,
                Type = complaint.Type,
                Description = complaint.Description,
                Status = complaint.Status,
                CreatedAt = complaint.CreatedAt,
                ResolvedAt = complaint.ResolvedAt
            };

            await SendAsync(response, 201, ct);
        }
    }
}
