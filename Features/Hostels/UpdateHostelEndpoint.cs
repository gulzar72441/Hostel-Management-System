using FastEndpoints;
using HostelManagementSystemApi.Features.Hostels.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class UpdateHostelEndpoint : Endpoint<UpdateHostelRequest>
    {
        private readonly ApplicationDbContext _context;

        public UpdateHostelEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/hostels/{HostelID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(UpdateHostelRequest req, CancellationToken ct)
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

            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.HostelID == req.HostelID && h.VendorID == vendor.VendorID, ct);

            if (hostel == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            hostel.Name = req.Name;
            hostel.Address = req.Address;
            hostel.City = req.City;
            hostel.State = req.State;
            hostel.Country = req.Country;
            hostel.Description = req.Description ?? hostel.Description;
            hostel.ContactPerson = req.ContactPerson;
            hostel.ContactEmail = req.ContactEmail;
            hostel.ContactPhone = req.ContactPhone;

            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
