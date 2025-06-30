namespace HostelManagementSystemApi.Domain
{
    public class Bed
    {
        public int BedID { get; set; }
        public int RoomID { get; set; }
        public virtual Room? Room { get; set; }
        public string BedNumber { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public int? StudentID { get; set; }
        public virtual Student? Student { get; set; }
    }
}
