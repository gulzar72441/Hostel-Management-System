using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.StaffAttendances
{
    public class DeleteStaffAttendanceEndpoint : EndpointWithoutRequest
    {
        private readonly ApplicationDbContext _context;

        public DeleteStaffAttendanceEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/staff-attendances/{StaffAttendanceID}");
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

            var attendanceId = Route<int>("StaffAttendanceID");
            var attendance = await _context.StaffAttendances
                .Include(a => a.Staff)!.ThenInclude(s => s!.Hostel)
                .FirstOrDefaultAsync(a => a.StaffAttendanceID == attendanceId, ct);

            if (attendance == null || attendance.Staff?.Hostel == null || attendance.Staff.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            _context.StaffAttendances.Remove(attendance);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
