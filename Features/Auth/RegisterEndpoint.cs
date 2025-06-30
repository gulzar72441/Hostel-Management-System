using FastEndpoints;
using HostelManagementSystemApi.Features.Auth.DTOs;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HostelManagementSystemApi.Features.Auth
{
    public class RegisterEndpoint : Endpoint<RegistrationRequest>
    {
        private readonly ApplicationDbContext _context;

        public RegisterEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/auth/register");
            AllowAnonymous();
        }

        public override async Task HandleAsync(RegistrationRequest req, CancellationToken ct)
        {
            if (await _context.Users.AnyAsync(u => u.Email == req.Email, ct))
            {
                await SendAsync(new { Message = "Email already exists." }, 409, ct);
                return;
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == req.Role, ct);
            if (role == null)
            {
                await SendAsync(new { Message = "Invalid role specified." }, 400, ct);
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

            if (role.RoleName == "Vendor")
            {
                if (string.IsNullOrWhiteSpace(req.CompanyName))
                {
                    await SendAsync(new { Message = "Company name is required for vendors." }, 400, ct);
                    return;
                }

                var vendor = new Vendor
                {
                    UserID = user.UserID,
                    CompanyName = req.CompanyName,
                    ContactInfo = user.Phone ?? string.Empty
                };
                await _context.Vendors.AddAsync(vendor, ct);
                await _context.SaveChangesAsync(ct);
            }

            await SendCreatedAtAsync<RegisterEndpoint>(new { userId = user.UserID }, null, cancellation: ct);
        }
    }
}
