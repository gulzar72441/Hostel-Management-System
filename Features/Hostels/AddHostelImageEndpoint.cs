using FastEndpoints;
using HostelManagementSystemApi.Domain;
using HostelManagementSystemApi.Features.Hostels.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class AddHostelImageEndpoint : Endpoint<AddHostelImageRequest>
    {
        private readonly ApplicationDbContext _context;

        public AddHostelImageEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/hostels/images");
            Roles("Vendor");
        }

        public override async Task HandleAsync(AddHostelImageRequest req, CancellationToken ct)
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

            var image = new HostelImage
            {
                HostelID = req.HostelID,
                ImageUrl = req.ImageUrl,
                Caption = req.Caption,
                IsPrimary = req.IsPrimary
            };

            await _context.HostelImages.AddAsync(image, ct);
            await _context.SaveChangesAsync(ct);

            await SendCreatedAtAsync<AddHostelImageEndpoint>(new { imageId = image.HostelImageID }, null, cancellation: ct);
        }
    }
}
