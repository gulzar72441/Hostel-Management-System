namespace HostelManagementSystemApi.Domain
{
    public class RoomType
    {
        public int RoomTypeID { get; set; }
        public int HostelID { get; set; }
        public virtual Hostel? Hostel { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
