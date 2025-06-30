using FastEndpoints;
using HostelManagementSystemApi.Features.Notices.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Notices
{
    public class UpdateNoticeEndpoint : Endpoint<UpdateNoticeRequest, NoticeResponse>
    {
        private readonly ApplicationDbContext _context;

        public UpdateNoticeEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/notices/{NoticeID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(UpdateNoticeRequest req, CancellationToken ct)
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

            var noticeId = Route<int>("NoticeID");
            var notice = await _context.Notices
                .Include(n => n.Hostel)
                .FirstOrDefaultAsync(n => n.NoticeID == noticeId, ct);

            if (notice == null || notice.Hostel == null || notice.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            notice.Title = req.Title;
            notice.Content = req.Content;
            notice.Audience = req.Audience;

            await _context.SaveChangesAsync(ct);

            var response = new NoticeResponse
            {
                NoticeID = notice.NoticeID,
                Title = notice.Title,
                Content = notice.Content,
                Audience = notice.Audience,
                Date = notice.Date,
                HostelName = notice.Hostel.Name
            };

            await SendAsync(response, 200, ct);
        }
    }
}
