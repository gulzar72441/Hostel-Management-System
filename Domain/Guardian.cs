namespace HostelManagementSystemApi.Domain
{
    public class Guardian
    {
        public int GuardianID { get; set; }
        public int? UserID { get; set; } // Nullable if a guardian can exist without a login
        public virtual User? User { get; set; }
        public int StudentID { get; set; }
        public virtual Student? Student { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Relation { get; set; } = string.Empty; // e.g., Father, Mother, etc.
    }
}
