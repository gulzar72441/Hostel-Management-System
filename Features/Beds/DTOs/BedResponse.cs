namespace HostelManagementSystemApi.Features.Beds.DTOs
{
    public class BedResponse
    {
        public int BedID { get; set; }
        public string BedNumber { get; set; } = string.Empty;
        public int RoomID { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public int? StudentID { get; set; }
        public string? StudentName { get; set; }
    }
}
