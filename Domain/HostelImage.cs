namespace HostelManagementSystemApi.Domain
{
    public class HostelImage
    {
        public int HostelImageID { get; set; }
        public int HostelID { get; set; }
        public virtual Hostel? Hostel { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public bool IsPrimary { get; set; }
    }
}
