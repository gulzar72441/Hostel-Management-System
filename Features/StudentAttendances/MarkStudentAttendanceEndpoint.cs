using FastEndpoints;
using HostelManagementSystemApi.Features.StudentAttendances.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HostelManagementSystemApi.Domain;

namespace HostelManagementSystemApi.Features.StudentAttendances
{
    public class MarkStudentAttendanceEndpoint : Endpoint<MarkStudentAttendanceRequest, StudentAttendanceResponse>
    {
        private readonly ApplicationDbContext _context;

        public MarkStudentAttendanceEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/student-attendances");
            Roles("Vendor");
        }

        public override async Task HandleAsync(MarkStudentAttendanceRequest req, CancellationToken ct)
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

            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Hostel)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentID == req.StudentID, ct);

            if (student == null || student.Hostel == null || student.Hostel.VendorID != vendor.VendorID)
            {
                AddError("Student not found or you do not have permission.");
                await SendErrorsAsync(404, ct);
                return;
            }

            var attendanceExists = await _context.Attendances
                .AnyAsync(a => a.StudentID == req.StudentID && a.Date.Date == req.Date.Date, ct);

            if (attendanceExists)
            {
                AddError("Attendance for this student on this date has already been marked.");
                await SendErrorsAsync(409, ct);
                return;
            }

            var newAttendance = new Attendance
            {
                StudentID = req.StudentID,
                Date = req.Date,
                Status = req.Status,
                CheckInTime = req.CheckInTime,
                CheckOutTime = req.CheckOutTime
            };

            _context.Attendances.Add(newAttendance);
            await _context.SaveChangesAsync(ct);

            var response = new StudentAttendanceResponse
            {
                AttendanceID = newAttendance.AttendanceID,
                StudentID = newAttendance.StudentID,
                StudentName = student.User!.Name,
                Date = newAttendance.Date,
                CheckInTime = newAttendance.CheckInTime,
                CheckOutTime = newAttendance.CheckOutTime,
                Status = newAttendance.Status
            };

            await SendAsync(response, 201, ct);
        }
    }
}
