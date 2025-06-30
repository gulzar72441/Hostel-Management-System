namespace HostelManagementSystemApi.Features.Staff.DTOs
{
    public class UpdateStaffRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Salary { get; set; }
    }
}
