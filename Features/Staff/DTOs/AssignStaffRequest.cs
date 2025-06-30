namespace HostelManagementSystemApi.Features.Staff.DTOs
{
    public class AssignStaffRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Manager" or "Warden"
        public int HostelID { get; set; }
        public string? Phone { get; set; }
    }
}
