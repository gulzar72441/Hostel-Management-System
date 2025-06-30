namespace HostelManagementSystemApi.Features.Auth.DTOs
{
    public class RegistrationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Student"; // Default role is Student
        public string? Phone { get; set; }
        public string? CompanyName { get; set; }
    }
}
