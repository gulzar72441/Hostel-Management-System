using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Staff
{
    public class DeleteStaffEndpoint : EndpointWithoutRequest
    {
        private readonly ApplicationDbContext _context;

        public DeleteStaffEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/staff/{StaffID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(CancellationToken ct)
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

            var staffId = Route<int>("StaffID");
            var staff = await _context.Staff
                .Include(s => s.Hostel)
                .FirstOrDefaultAsync(s => s.StaffID == staffId, ct);

            if (staff == null || staff.Hostel == null || staff.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var user = await _context.Users.FindAsync(new object[] { staff.UserID }, cancellationToken: ct);

            _context.Staff.Remove(staff);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
