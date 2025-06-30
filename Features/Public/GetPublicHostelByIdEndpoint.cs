using FastEndpoints;
using HostelManagementSystemApi.Features.Hostels.DTOs.Public;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HostelManagementSystemApi.Features.Public
{
    public class GetPublicHostelByIdRequest
    {
        public int HostelID { get; set; }
    }

    public class GetPublicHostelByIdEndpoint : Endpoint<GetPublicHostelByIdRequest, PublicHostelResponse>
    {
        private readonly ApplicationDbContext _context;

        public GetPublicHostelByIdEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/public/hostels/{HostelID}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetPublicHostelByIdRequest req, CancellationToken ct)
        {
            var hostel = await _context.Hostels
                .Where(h => h.IsApproved && h.HostelID == req.HostelID)
                .Select(h => new PublicHostelResponse
                {
                    HostelID = h.HostelID,
                    Name = h.Name,
                    Address = h.Address,
                    City = h.City,
                    State = h.State,
                    Country = h.Country,
                    Description = h.Description,
                    ContactPerson = h.ContactPerson,
                    ContactEmail = h.ContactEmail,
                    ContactPhone = h.ContactPhone
                })
                .FirstOrDefaultAsync(ct);

            if (hostel == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendAsync(hostel, 200, ct);
        }
    }
}
