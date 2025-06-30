using FastEndpoints;
using HostelManagementSystemApi.Features.Complaints.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Complaints
{
    public class UpdateComplaintStatusEndpoint : Endpoint<UpdateComplaintStatusRequest, ComplaintResponse>
    {
        private readonly ApplicationDbContext _context;

        public UpdateComplaintStatusEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/complaints/{ComplaintID}/status");
            Roles("Vendor");
        }

        public override async Task HandleAsync(UpdateComplaintStatusRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var vendor = await _context.Vendors.AsNoTracking().FirstOrDefaultAsync(v => v.UserID == int.Parse(userId), ct);
            if (vendor == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var complaintId = Route<int>("ComplaintID");
            var complaint = await _context.Complaints
                .Include(c => c.Student)!.ThenInclude(s => s!.User)
                .Include(c => c.Hostel)
                .FirstOrDefaultAsync(c => c.ComplaintID == complaintId, ct);

            if (complaint == null || complaint.Hostel == null || complaint.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            complaint.Status = req.Status;
            if (req.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
            {
                complaint.ResolvedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);

            var response = new ComplaintResponse
            {
                ComplaintID = complaint.ComplaintID,
                StudentID = complaint.StudentID,
                StudentName = complaint.Student!.User!.Name,
                HostelID = complaint.HostelID,
                HostelName = complaint.Hostel.Name,
                Type = complaint.Type,
                Description = complaint.Description,
                Status = complaint.Status,
                CreatedAt = complaint.CreatedAt,
                ResolvedAt = complaint.ResolvedAt
            };

            await SendAsync(response, 200, ct);
        }
    }
}
