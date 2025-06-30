using FastEndpoints;
using HostelManagementSystemApi.Features.Hostels.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Hostels
{
    public class GetAllAmenitiesEndpoint : EndpointWithoutRequest<List<AmenityResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllAmenitiesEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/amenities");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var amenities = await _context.Amenities.AsNoTracking()
                .Select(a => new AmenityResponse
                {
                    AmenityID = a.AmenityID,
                    Name = a.Name,
                    Description = a.Description
                })
                .ToListAsync(ct);

            await SendAsync(amenities, 200, ct);
        }
    }
}
