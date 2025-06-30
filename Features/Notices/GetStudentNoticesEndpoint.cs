using FastEndpoints;
using HostelManagementSystemApi.Features.Notices.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Notices
{
    public class GetStudentNoticesEndpoint : EndpointWithoutRequest<List<NoticeResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetStudentNoticesEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/notices/my-hostel");
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

            var notices = await _context.Notices
                .Where(n => n.HostelID == student.HostelID)
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
