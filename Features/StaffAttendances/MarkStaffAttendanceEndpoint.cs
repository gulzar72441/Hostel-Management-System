using FastEndpoints;
using HostelManagementSystemApi.Features.StaffAttendances.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.StaffAttendances
{
    public class MarkStaffAttendanceEndpoint : Endpoint<MarkAttendanceRequest, StaffAttendanceResponse>
    {
        private readonly ApplicationDbContext _context;

        public MarkStaffAttendanceEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/staff-attendances");
            Roles("Vendor");
        }

        public override async Task HandleAsync(MarkAttendanceRequest req, CancellationToken ct)
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

            var staff = await _context.Staff
                .Include(s => s.User)
                .Include(s => s.Hostel)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StaffID == req.StaffID, ct);

            if (staff == null || staff.Hostel == null || staff.Hostel.VendorID != vendor.VendorID)
            {
                AddError("Staff member not found or you do not have permission.");
                await SendErrorsAsync(404, ct);
                return;
            }

            var attendanceExists = await _context.StaffAttendances
                .AnyAsync(a => a.StaffID == req.StaffID && a.Date.Date == req.Date.Date, ct);

            if (attendanceExists)
            {
                AddError("Attendance for this staff member on this date has already been marked.");
                await SendErrorsAsync(409, ct);
                return;
            }

            var newAttendance = new Domain.StaffAttendance
            {
                StaffID = req.StaffID,
                Date = req.Date,
                Status = req.Status,
                CheckInTime = req.CheckInTime,
                CheckOutTime = req.CheckOutTime
            };

            _context.StaffAttendances.Add(newAttendance);
            await _context.SaveChangesAsync(ct);

            var response = new StaffAttendanceResponse
            {
                StaffAttendanceID = newAttendance.StaffAttendanceID,
                StaffID = newAttendance.StaffID,
                StaffName = staff.User!.Name,
                Date = newAttendance.Date,
                CheckInTime = newAttendance.CheckInTime,
                CheckOutTime = newAttendance.CheckOutTime,
                Status = newAttendance.Status
            };

            await SendAsync(response, 201, ct);
        }
    }
}
