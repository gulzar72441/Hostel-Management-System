using FastEndpoints;
using HostelManagementSystemApi.Features.Notices.DTOs;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Notices
{
    public class CreateNoticeEndpoint : Endpoint<CreateNoticeRequest, NoticeResponse>
    {
        private readonly ApplicationDbContext _context;

        public CreateNoticeEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/notices");
            Roles("Vendor");
        }

        public override async Task HandleAsync(CreateNoticeRequest req, CancellationToken ct)
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

            var hostel = await _context.Hostels.AsNoTracking().FirstOrDefaultAsync(h => h.HostelID == req.HostelID, ct);
            if (hostel == null || hostel.VendorID != vendor.VendorID)
            {
                AddError("Hostel not found or you do not have permission to post a notice here.");
                await SendErrorsAsync(404, ct);
                return;
            }

            var notice = new Notice
            {
                HostelID = req.HostelID,
                Title = req.Title,
                Content = req.Content,
                Audience = req.Audience,
                Date = DateTime.UtcNow
            };

            _context.Notices.Add(notice);
            await _context.SaveChangesAsync(ct);

            var response = new NoticeResponse
            {
                NoticeID = notice.NoticeID,
                Title = notice.Title,
                Content = notice.Content,
                Audience = notice.Audience,
                Date = notice.Date,
                HostelName = hostel.Name
            };

            await SendAsync(response, 201, ct);
        }
    }
}
