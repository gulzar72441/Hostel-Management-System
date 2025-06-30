namespace HostelManagementSystemApi.Features.Beds.DTOs
{
    public class CreateBedRequest
    {
        public int RoomID { get; set; }
        public string BedNumber { get; set; } = string.Empty;
    }
}
