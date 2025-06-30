using FastEndpoints;
using HostelManagementSystemApi.Features.StaffAttendances.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.StaffAttendances
{
    public class GetStaffAttendanceHistoryEndpoint : EndpointWithoutRequest<List<StaffAttendanceResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetStaffAttendanceHistoryEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/staff/{StaffID}/attendances");
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

            var staffId = Route<int>("StaffID");
            var staff = await _context.Staff
                .Include(s => s.User)
                .Include(s => s.Hostel)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StaffID == staffId, ct);

            if (staff == null || staff.Hostel == null || staff.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var attendances = await _context.StaffAttendances
                .Where(a => a.StaffID == staffId)
                .AsNoTracking()
                .Select(a => new StaffAttendanceResponse
                {
                    StaffAttendanceID = a.StaffAttendanceID,
                    StaffID = a.StaffID,
                    StaffName = staff.User!.Name,
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
