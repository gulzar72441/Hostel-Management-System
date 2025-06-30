using System.ComponentModel.DataAnnotations;

namespace HostelManagementSystemApi.Domain
{
    public class SystemSetting
    {
        [Key]
        public int SystemSettingId { get; set; }
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
    }
}
