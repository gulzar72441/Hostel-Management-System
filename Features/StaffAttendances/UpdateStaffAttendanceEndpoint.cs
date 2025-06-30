using FastEndpoints;
using HostelManagementSystemApi.Features.StaffAttendances.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.StaffAttendances
{
    public class UpdateStaffAttendanceEndpoint : Endpoint<UpdateAttendanceRequest, StaffAttendanceResponse>
    {
        private readonly ApplicationDbContext _context;

        public UpdateStaffAttendanceEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/staff-attendances/{StaffAttendanceID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(UpdateAttendanceRequest req, CancellationToken ct)
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

            var attendanceId = Route<int>("StaffAttendanceID");
            var attendance = await _context.StaffAttendances
                .Include(a => a.Staff)!.ThenInclude(s => s!.Hostel)
                .Include(a => a.Staff)!.ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(a => a.StaffAttendanceID == attendanceId, ct);

            if (attendance == null || attendance.Staff?.Hostel == null || attendance.Staff.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            attendance.Date = req.Date;
            attendance.Status = req.Status;
            attendance.CheckInTime = req.CheckInTime;
            attendance.CheckOutTime = req.CheckOutTime;

            await _context.SaveChangesAsync(ct);

            var response = new StaffAttendanceResponse
            {
                StaffAttendanceID = attendance.StaffAttendanceID,
                StaffID = attendance.StaffID,
                StaffName = attendance.Staff.User!.Name,
                Date = attendance.Date,
                CheckInTime = attendance.CheckInTime,
                CheckOutTime = attendance.CheckOutTime,
                Status = attendance.Status
            };

            await SendAsync(response, 200, ct);
        }
    }
}
