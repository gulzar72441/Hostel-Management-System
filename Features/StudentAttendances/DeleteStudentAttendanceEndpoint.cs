using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.StudentAttendances
{
    public class DeleteStudentAttendanceEndpoint : EndpointWithoutRequest
    {
        private readonly ApplicationDbContext _context;

        public DeleteStudentAttendanceEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/student-attendances/{AttendanceID}");
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

            var attendanceId = Route<int>("AttendanceID");
            var attendance = await _context.Attendances
                .Include(a => a.Student)!.ThenInclude(s => s!.Hostel)
                .FirstOrDefaultAsync(a => a.AttendanceID == attendanceId, ct);

            if (attendance == null || attendance.Student?.Hostel == null || attendance.Student.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
