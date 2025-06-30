using FastEndpoints;
using HostelManagementSystemApi.Features.Inventory.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Inventory
{
    public class UpdateInventoryItemEndpoint : Endpoint<UpdateInventoryItemRequest, InventoryResponse>
    {
        private readonly ApplicationDbContext _context;

        public UpdateInventoryItemEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Put("/api/inventory/{InventoryID}");
            Roles("Vendor");
        }

        public override async Task HandleAsync(UpdateInventoryItemRequest req, CancellationToken ct)
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

            inventoryItem.ItemName = req.ItemName;
            inventoryItem.Quantity = req.Quantity;
            inventoryItem.Unit = req.Unit;

            await _context.SaveChangesAsync(ct);

            var response = new InventoryResponse
            {
                InventoryID = inventoryItem.InventoryID,
                ItemName = inventoryItem.ItemName,
                Quantity = inventoryItem.Quantity,
                Unit = inventoryItem.Unit,
                HostelID = inventoryItem.HostelID,
                HostelName = inventoryItem.Hostel.Name
            };

            await SendAsync(response, 200, ct);
        }
    }
}
