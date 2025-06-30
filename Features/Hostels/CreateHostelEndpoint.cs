using FastEndpoints;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Features.Hostels.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class CreateHostelEndpoint : Endpoint<CreateHostelRequest, int>
    {
        private readonly ApplicationDbContext _context;

        public CreateHostelEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/hostels");
            Roles("Vendor");
        }

        public override async Task HandleAsync(CreateHostelRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserID == int.Parse(userId), ct);
            if (vendor == null)
            {
                // This case should ideally not happen if the role is correctly assigned.
                await SendForbiddenAsync(ct);
                return;
            }

            var hostel = new Hostel
            {
                Name = req.Name,
                Address = req.Address,
                City = req.City,
                State = req.State,
                Country = req.Country,
                Description = req.Description ?? string.Empty,
                ContactPerson = req.ContactPerson,
                ContactEmail = req.ContactEmail,
                ContactPhone = req.ContactPhone,
                VendorID = vendor.VendorID,
                DateListed = DateTime.UtcNow,
                IsApproved = false // Hostels require admin approval by default
            };

            _context.Hostels.Add(hostel);
            await _context.SaveChangesAsync(ct);

            await SendAsync(hostel.HostelID, 201, ct);
        }
    }
}
