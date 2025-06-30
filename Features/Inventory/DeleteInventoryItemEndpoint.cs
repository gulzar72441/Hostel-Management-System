using FastEndpoints;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Inventory
{
    public class DeleteInventoryItemEndpoint : EndpointWithoutRequest
    {
        private readonly ApplicationDbContext _context;

        public DeleteInventoryItemEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/inventory/{InventoryID}");
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

            var inventoryId = Route<int>("InventoryID");
            var inventoryItem = await _context.Inventories
                .Include(i => i.Hostel)
                .FirstOrDefaultAsync(i => i.InventoryID == inventoryId, ct);

            if (inventoryItem == null || inventoryItem.Hostel == null || inventoryItem.Hostel.VendorID != vendor.VendorID)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            _context.Inventories.Remove(inventoryItem);
            await _context.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
