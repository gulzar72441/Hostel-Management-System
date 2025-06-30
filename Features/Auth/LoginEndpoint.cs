using FastEndpoints;
using HostelManagementSystemApi.Features.Auth.DTOs;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HostelManagementSystemApi.Features.Auth
{
    public class LoginEndpoint : Endpoint<LoginRequest, AuthResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public LoginEndpoint(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public override void Configure()
        {
            Post("/api/auth/login");
            AllowAnonymous();
        }

        public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == req.Email, ct);

            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var token = GenerateJwtToken(user);

            await SendAsync(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Name = user.Name
            }, cancellation: ct);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.Role?.RoleName ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
