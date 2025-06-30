using System;

using System.ComponentModel.DataAnnotations;

namespace HostelManagementSystemApi.Domain
{
    public class SystemLog
    {
        [Key]
        public int SystemLogId { get; set; }
        public string LogLevel { get; set; } = string.Empty; // e.g., Info, Warning, Error
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
