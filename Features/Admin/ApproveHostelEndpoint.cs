using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HostelManagementSystemApi.Features.Admin
{
    public class ApproveHostelRequest
    {
        public int HostelID { get; set; }
    }

    public class ApproveHostelEndpoint : Endpoint<ApproveHostelRequest>
    {
        private readonly ApplicationDbContext _context;

        public ApproveHostelEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/admin/hostels/{HostelID}/approve");
            Roles("Admin");
        }

        public override async Task HandleAsync(ApproveHostelRequest req, CancellationToken ct)
        {
            var hostel = await _context.Hostels.FirstOrDefaultAsync(h => h.HostelID == req.HostelID, ct);

            if (hostel == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            hostel.IsApproved = true;
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
