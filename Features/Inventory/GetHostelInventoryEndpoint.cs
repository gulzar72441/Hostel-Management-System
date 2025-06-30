using FastEndpoints;
using HostelManagementSystemApi.Features.Inventory.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Inventory
{
    public class GetHostelInventoryEndpoint : EndpointWithoutRequest<List<InventoryResponse>>
    {
        private readonly ApplicationDbContext _context;

        public GetHostelInventoryEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/hostels/{HostelID}/inventory");
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

            var hostelId = Route<int>("HostelID");
            var hostel = await _context.Hostels.AsNoTracking().FirstOrDefaultAsync(h => h.HostelID == hostelId, ct);

            if (hostel == null || hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var inventoryList = await _context.Inventories
                .Where(i => i.HostelID == hostelId)
                .Include(i => i.Hostel)
                .AsNoTracking()
                .Select(i => new InventoryResponse
                {
                    InventoryID = i.InventoryID,
                    ItemName = i.ItemName,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    HostelID = i.HostelID,
                    HostelName = i.Hostel!.Name
                })
                .ToListAsync(ct);

            await SendAsync(inventoryList, 200, ct);
        }
    }
}
