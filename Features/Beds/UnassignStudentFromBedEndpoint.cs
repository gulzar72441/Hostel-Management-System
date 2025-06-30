using FastEndpoints;
using HostelManagementSystemApi.Features.Beds.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Beds
{
    public class UnassignStudentFromBedEndpoint : EndpointWithoutRequest<BedResponse>
    {
        private readonly ApplicationDbContext _context;

        public UnassignStudentFromBedEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/beds/{BedID}/unassign");
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

            var bedId = Route<int>("BedID");
            var bed = await _context.Beds
                .Include(b => b.Room)
                .ThenInclude(r => r!.Hostel)
                .FirstOrDefaultAsync(b => b.BedID == bedId, ct);

            if (bed == null || bed.Room?.Hostel?.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (!bed.IsOccupied)
            {
                AddError("This bed is not currently occupied.");
                await SendErrorsAsync(409, ct);
                return;
            }

            bed.StudentID = null;
            bed.IsOccupied = false;
            await _context.SaveChangesAsync(ct);

            var response = new BedResponse
            {
                BedID = bed.BedID,
                BedNumber = bed.BedNumber,
                RoomID = bed.RoomID,
                RoomNumber = bed.Room!.RoomNumber,
                IsOccupied = bed.IsOccupied,
                StudentID = bed.StudentID,
                StudentName = null
            };

            await SendAsync(response, 200, ct);
        }
    }
}
