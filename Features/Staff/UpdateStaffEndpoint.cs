using FastEndpoints;
using HostelManagementSystemApi.Features.Staff.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Staff
{
    public class UpdateStaffEndpoint : Endpoint<UpdateStaffRequest, StaffResponse>
    {
        private readonly ApplicationDbContext _context;

        public UpdateStaffEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/staff/{StaffID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(UpdateStaffRequest req, CancellationToken ct)
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
                .Include(s => s.Hostel)
                .Include(s => s.User)!.ThenInclude(u => u!.Role)
                .FirstOrDefaultAsync(s => s.StaffID == staffId, ct);

            if (staff == null || staff.Hostel == null || staff.User == null || staff.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            // Check if the new email is already in use by another user
            if (staff.User.Email != req.Email && await _context.Users.AnyAsync(u => u.Email == req.Email, ct))
            {
                AddError("A user with this email already exists.");
                await SendErrorsAsync(409, ct);
                return;
            }

            staff.User.Name = req.Name;
            staff.User.Email = req.Email;
            staff.Salary = req.Salary;

            await _context.SaveChangesAsync(ct);

            var response = new StaffResponse
            {
                StaffID = staff.StaffID,
                UserID = staff.UserID,
                Name = staff.User.Name,
                Email = staff.User.Email,
                Role = staff.User.Role!.RoleName,
                Salary = staff.Salary,
                JoinDate = staff.JoinDate,
                HostelID = staff.HostelID,
                HostelName = staff.Hostel.Name
            };

            await SendAsync(response, 200, ct);
        }
    }
}
