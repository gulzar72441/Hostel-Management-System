using System;

namespace HostelManagementSystemApi.Features.Staff.DTOs
{
    public class StaffResponse
    {
        public int StaffID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int HostelID { get; set; }
        public decimal Salary { get; set; }
        public DateTime JoinDate { get; set; }
        public string HostelName { get; set; } = string.Empty;
    }
}
