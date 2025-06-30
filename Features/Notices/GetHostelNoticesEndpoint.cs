using FastEndpoints;
using HostelManagementSystemApi.Features.Notices.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Notices
{
    public class GetHostelNoticesEndpoint : EndpointWithoutRequest<List<NoticeResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetHostelNoticesEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/hostels/{HostelID}/notices");
            Roles("Vendor");
        }

        public override async Task HandleAsync(CancellationToken ct)
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

            var hostelId = Route<int>("HostelID");
            var hostel = await _context.Hostels.AsNoTracking().FirstOrDefaultAsync(h => h.HostelID == hostelId, ct);

            if (hostel == null || hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var notices = await _context.Notices
                .Where(n => n.HostelID == hostelId)
                .Include(n => n.Hostel)
                .AsNoTracking()
                .Select(n => new NoticeResponse
                {
                    NoticeID = n.NoticeID,
                    Title = n.Title,
                    Content = n.Content,
                    Audience = n.Audience,
                    Date = n.Date,
                    HostelName = n.Hostel!.Name
                })
                .ToListAsync(ct);

            await SendAsync(notices, 200, ct);
        }
    }
}
