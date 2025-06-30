using FastEndpoints;
using HostelManagementSystemApi.Features.StudentAttendances.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.StudentAttendances
{
    public class GetStudentAttendanceHistoryEndpoint : EndpointWithoutRequest<List<StudentAttendanceResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetStudentAttendanceHistoryEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/students/{StudentID}/attendances");
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

            var studentId = Route<int>("StudentID");
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Hostel)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentID == studentId, ct);

            if (student == null || student.Hostel == null || student.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var attendances = await _context.Attendances
                .Where(a => a.StudentID == studentId)
                .AsNoTracking()
                .Select(a => new StudentAttendanceResponse
                {
                    AttendanceID = a.AttendanceID,
                    StudentID = a.StudentID,
                    StudentName = student.User!.Name,
                    Date = a.Date,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    Status = a.Status
                })
                .ToListAsync(ct);

            await SendAsync(attendances, 200, ct);
        }
    }
}
