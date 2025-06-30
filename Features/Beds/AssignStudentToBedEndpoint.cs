using FastEndpoints;
using HostelManagementSystemApi.Features.Beds.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Features.Beds
{
    public class AssignStudentToBedEndpoint : Endpoint<AssignStudentToBedRequest, BedResponse>
    {
        private readonly ApplicationDbContext _context;

        public AssignStudentToBedEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/beds/{BedID}/assign");
            Roles("Vendor");
        }

        public override async Task HandleAsync(AssignStudentToBedRequest req, CancellationToken ct)
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

            if (bed.IsOccupied)
            {
                AddError("This bed is already occupied.");
                await SendErrorsAsync(409, ct);
                return;
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentID == req.StudentID, ct);

            if (student == null || student.HostelID != bed.Room.HostelID || student.User == null)
            {
                AddError("Student not found or not registered in this hostel.");
                await SendErrorsAsync(404, ct);
                return;
            }

            var isStudentAlreadyAssigned = await _context.Beds.AnyAsync(b => b.StudentID == req.StudentID, ct);
            if (isStudentAlreadyAssigned)
            {
                AddError("This student is already assigned to another bed.");
                await SendErrorsAsync(409, ct);
                return;
            }

            bed.StudentID = req.StudentID;
            bed.IsOccupied = true;
            await _context.SaveChangesAsync(ct);

            var response = new BedResponse
            {
                BedID = bed.BedID,
                BedNumber = bed.BedNumber,
                RoomID = bed.RoomID,
                RoomNumber = bed.Room.RoomNumber,
                IsOccupied = bed.IsOccupied,
                StudentID = bed.StudentID,
                StudentName = student.User.Name
            };

            await SendAsync(response, 200, ct);
        }
    }
}
