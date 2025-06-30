using FastEndpoints;
using HostelManagementSystemApi.Features.StudentAttendances.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.StudentAttendances
{
    public class UpdateStudentAttendanceEndpoint : Endpoint<UpdateStudentAttendanceRequest, StudentAttendanceResponse>
    {
        private readonly ApplicationDbContext _context;

        public UpdateStudentAttendanceEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/student-attendances/{AttendanceID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(UpdateStudentAttendanceRequest req, CancellationToken ct)
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
                .Include(a => a.Student)!.ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(a => a.AttendanceID == attendanceId, ct);

            if (attendance == null || attendance.Student?.Hostel == null || attendance.Student.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            attendance.Date = req.Date;
            attendance.Status = req.Status;
            attendance.CheckInTime = req.CheckInTime;
            attendance.CheckOutTime = req.CheckOutTime;

            await _context.SaveChangesAsync(ct);

            var response = new StudentAttendanceResponse
            {
                AttendanceID = attendance.AttendanceID,
                StudentID = attendance.StudentID,
                StudentName = attendance.Student.User!.Name,
                Date = attendance.Date,
                CheckInTime = attendance.CheckInTime,
                CheckOutTime = attendance.CheckOutTime,
                Status = attendance.Status
            };

            await SendAsync(response, 200, ct);
        }
    }
}
