using System.ComponentModel.DataAnnotations;

namespace HostelManagementSystemApi.Domain
{
    public class NotificationPreference
    {
        [Key]
        public int NotificationPreferenceId { get; set; }
        public int UserID { get; set; }
        public virtual User? User { get; set; }
        public string NotificationType { get; set; } = string.Empty; // e.g., Fee-Reminder, Notice, Complaint-Update
        public bool IsEmailEnabled { get; set; }
        public bool IsSMSEnabled { get; set; }
        public bool IsPushEnabled { get; set; }
    }
}
