using FastEndpoints;
using HostelManagementSystemApi.Features.Inventory.DTOs;
using HostelManagementSystemApi.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HostelManagementSystemApi.Features.Inventory
{
    public class AddInventoryItemEndpoint : Endpoint<AddInventoryItemRequest, InventoryResponse>
    {
        private readonly ApplicationDbContext _context;

        public AddInventoryItemEndpoint(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Post("/api/inventory");
            Roles("Vendor");
        }

        public override async Task HandleAsync(AddInventoryItemRequest req, CancellationToken ct)
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

            var hostel = await _context.Hostels.AsNoTracking().FirstOrDefaultAsync(h => h.HostelID == req.HostelID, ct);
            if (hostel == null || hostel.VendorID != vendor.VendorID)
            {
                AddError("Hostel not found or you do not have permission to add inventory here.");
                await SendErrorsAsync(404, ct);
                return;
            }

            var inventoryItem = new Domain.Inventory
            {
                HostelID = req.HostelID,
                ItemName = req.ItemName,
                Quantity = req.Quantity,
                Unit = req.Unit
            };

            _context.Inventories.Add(inventoryItem);
            await _context.SaveChangesAsync(ct);

            var response = new InventoryResponse
            {
                InventoryID = inventoryItem.InventoryID,
                ItemName = inventoryItem.ItemName,
                Quantity = inventoryItem.Quantity,
                Unit = inventoryItem.Unit,
                HostelID = hostel.HostelID,
                HostelName = hostel.Name
            };

            await SendAsync(response, 201, ct);
        }
    }
}
