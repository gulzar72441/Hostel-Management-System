using FastEndpoints;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Features.Staff.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Staff
{
    public class AssignStaffEndpoint : Endpoint<AssignStaffRequest>
    {
        private readonly ApplicationDbContext _context;

        public AssignStaffEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/staff/assign");
            Roles("Vendor");
        }

        public override async Task HandleAsync(AssignStaffRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var vendor = await _context.Vendors.AsNoTracking()
                .FirstOrDefaultAsync(v => v.UserID == int.Parse(userId), ct);

            if (vendor == null)
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.HostelID == req.HostelID && h.VendorID == vendor.VendorID, ct);

            if (hostel == null)
            {
                await SendAsync(new { Message = "Hostel not found or you do not have permission to manage it." }, 404, ct);
                return;
            }

            if (await _context.Users.AnyAsync(u => u.Email == req.Email, ct))
            {
                await SendAsync(new { Message = "Email already exists." }, 409, ct);
                return;
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == req.Role, ct);
            if (role == null || (role.RoleName != "Manager" && role.RoleName != "Warden"))
            {
                await SendAsync(new { Message = "Invalid staff role specified." }, 400, ct);
                return;
            }

            var user = new User
            {
                Name = req.Name,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                RoleID = role.RoleID,
                Phone = req.Phone,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct); // Save to get the UserID

            var staff = new Domain.Staff
            {
                UserID = user.UserID,
                HostelID = req.HostelID,
                JoinDate = DateTime.UtcNow
            };

            await _context.Staff.AddAsync(staff, ct);
            await _context.SaveChangesAsync(ct);

            await SendCreatedAtAsync<AssignStaffEndpoint>(new { staffId = staff.StaffID }, null, cancellation: ct);
        }
    }
}
