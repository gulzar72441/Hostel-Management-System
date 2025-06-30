using FastEndpoints;
using HostelManagementSystemApi.Features.Staff.DTOs;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Staff
{
    public class AddStaffEndpoint : Endpoint<AddStaffRequest, StaffResponse>
    {
        private readonly ApplicationDbContext _context;

        public AddStaffEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/staff");
            Roles("Vendor");
        }

        public override async Task HandleAsync(AddStaffRequest req, CancellationToken ct)
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

            var hostel = await _context.Hostels.AsNoTracking().FirstOrDefaultAsync(h => h.HostelID == req.HostelID, ct);
            if (hostel == null || hostel.VendorID != vendor.VendorID)
            {
                AddError("Hostel not found or you do not have permission to add staff here.");
                await SendErrorsAsync(404, ct);
                return;
            }

            if (await _context.Users.AnyAsync(u => u.Email == req.Email, ct))
            {
                AddError("A user with this email already exists.");
                await SendErrorsAsync(409, ct);
                return;
            }

            var staffRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Staff", ct);
            if (staffRole == null)
            {
                ThrowError("Staff role not found in the database.");
                return;
            }

            var newUser = new User
            {
                Name = req.Name,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                RoleID = staffRole.RoleID,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(ct); // Save to get the UserID

            var newStaff = new Domain.Staff
            {
                UserID = newUser.UserID,
                HostelID = req.HostelID,
                Salary = req.Salary,
                JoinDate = req.JoinDate
            };

            _context.Staff.Add(newStaff);
            await _context.SaveChangesAsync(ct);

            var response = new StaffResponse
            {
                StaffID = newStaff.StaffID,
                UserID = newUser.UserID,
                Name = newUser.Name,
                Email = newUser.Email,
                Role = staffRole.RoleName,
                Salary = newStaff.Salary,
                JoinDate = newStaff.JoinDate,
                HostelID = hostel.HostelID,
                HostelName = hostel.Name
            };

            await SendAsync(response, 201, ct);
        }
    }
}
