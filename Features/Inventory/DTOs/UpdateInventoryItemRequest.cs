namespace HostelManagementSystemApi.Features.Inventory.DTOs
{
    public class UpdateInventoryItemRequest
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
