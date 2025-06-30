using HostelManagementSystemApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace HostelManagementSystemApi.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Hostel> Hostels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<StaffAttendance> StaffAttendances { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<HostelReview> HostelReviews { get; set; }
        public DbSet<AttendanceReason> AttendanceReasons { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }
        public DbSet<StudentActivityLog> StudentActivityLogs { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<HostelImage> HostelImages { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Amenity> Amenities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User-Role relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID);

            // Vendor-User relationship (one-to-one)
            modelBuilder.Entity<Vendor>()
                .HasOne(v => v.User)
                .WithOne(u => u.Vendor)
                .HasForeignKey<Vendor>(v => v.UserID);

            // Student-User relationship (one-to-one)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserID);

            // Staff-User relationship (one-to-one)
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.User)
                .WithOne(u => u.Staff)
                .HasForeignKey<Staff>(s => s.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Hostel-Vendor relationship
            modelBuilder.Entity<Hostel>()
                .HasOne(h => h.Vendor)
                .WithMany(v => v.Hostels)
                .HasForeignKey(h => h.VendorID);

            // Room-Hostel relationship
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Hostel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HostelID);

            // Room-RoomType relationship
            modelBuilder.Entity<Room>()
                .HasOne(r => r.RoomType)
                .WithMany()
                .HasForeignKey(r => r.RoomTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            // Bed-Room relationship
            modelBuilder.Entity<Bed>()
                .HasOne(b => b.Room)
                .WithMany(r => r.Beds)
                .HasForeignKey(b => b.RoomID);

            // Student-Hostel relationship
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Hostel)
                .WithMany(h => h.Students)
                .HasForeignKey(s => s.HostelID)
                .OnDelete(DeleteBehavior.Restrict);

            // Bed-Student relationship (one-to-one)
            modelBuilder.Entity<Bed>()
                .HasOne(b => b.Student)
                .WithOne(s => s.Bed)
                .HasForeignKey<Bed>(b => b.StudentID)
                .IsRequired(false) // A bed is not required to have a student
                .OnDelete(DeleteBehavior.Restrict); // When a student is deleted, set StudentID in Bed to null

            // ChatMessage relationships
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Recipient)
                .WithMany()
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking-Student relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Student)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking-Room relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomID);

            // Hostel-HostelImage relationship
            modelBuilder.Entity<HostelImage>()
                .HasOne(hi => hi.Hostel)
                .WithMany(h => h.HostelImages)
                .HasForeignKey(hi => hi.HostelID);

            // Hostel-RoomType relationship
            modelBuilder.Entity<RoomType>()
                .HasOne(rt => rt.Hostel)
                .WithMany(h => h.RoomTypes)
                .HasForeignKey(rt => rt.HostelID);

            // Guardian-User relationship (one-to-one)
            modelBuilder.Entity<Guardian>()
                .HasOne(g => g.User)
                .WithOne(u => u.Guardian)
                .HasForeignKey<Guardian>(g => g.UserID);

            // Guardian-Student relationship
            modelBuilder.Entity<Guardian>()
                .HasOne(g => g.Student)
                .WithMany(s => s.Guardians)
                .HasForeignKey(g => g.StudentID);

            // Attendance-Student relationship
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentID);

            // Payment-Student relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Student)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment-Hostel relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Hostel)
                .WithMany(h => h.Payments)
                .HasForeignKey(p => p.HostelID);

            // Notice-Hostel relationship
            modelBuilder.Entity<Notice>()
                .HasOne(n => n.Hostel)
                .WithMany(h => h.Notices)
                .HasForeignKey(n => n.HostelID);

            // Complaint-Student relationship
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Student)
                .WithMany(s => s.Complaints)
                .HasForeignKey(c => c.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            // Complaint-Hostel relationship
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Hostel)
                .WithMany(h => h.Complaints)
                .HasForeignKey(c => c.HostelID);

            // Staff-Hostel relationship
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Hostel)
                .WithMany(h => h.Staff)
                .HasForeignKey(s => s.HostelID);

            // StaffAttendance-Staff relationship
            modelBuilder.Entity<StaffAttendance>()
                .HasOne(sa => sa.Staff)
                .WithMany(s => s.StaffAttendances)
                .HasForeignKey(sa => sa.StaffID);

            // Inventory-Hostel relationship
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Hostel)
                .WithMany(h => h.Inventory)
                .HasForeignKey(i => i.HostelID);

            // InventoryLog-Inventory relationship
            modelBuilder.Entity<InventoryLog>()
                .HasOne(il => il.Inventory)
                .WithMany(i => i.InventoryLogs)
                .HasForeignKey(il => il.InventoryID);

            // InventoryLog-Staff relationship
            modelBuilder.Entity<InventoryLog>()
                .HasOne(il => il.Staff)
                .WithMany(s => s.InventoryLogs)
                .HasForeignKey(il => il.StaffID)
                .OnDelete(DeleteBehavior.Restrict);

            // PaymentTransaction-Payment relationship
            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(pt => pt.Payment)
                .WithMany(p => p.PaymentTransactions)
                .HasForeignKey(pt => pt.PaymentID);

            // HostelReview-Hostel relationship
            modelBuilder.Entity<HostelReview>()
                .HasOne(hr => hr.Hostel)
                .WithMany(h => h.HostelReviews)
                .HasForeignKey(hr => hr.HostelID);

            // HostelReview-Student relationship
            modelBuilder.Entity<HostelReview>()
                .HasOne(hr => hr.Student)
                .WithMany(s => s.HostelReviews)
                .HasForeignKey(hr => hr.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            // AttendanceReason-Attendance relationship
            modelBuilder.Entity<AttendanceReason>()
                .HasOne(ar => ar.Attendance)
                .WithMany(a => a.AttendanceReasons)
                .HasForeignKey(ar => ar.AttendanceID);

            // NotificationPreference-User relationship
            modelBuilder.Entity<NotificationPreference>()
                .HasOne(np => np.User)
                .WithMany(u => u.NotificationPreferences)
                .HasForeignKey(np => np.UserID);

            // StudentActivityLog-Student relationship
            modelBuilder.Entity<StudentActivityLog>()
                .HasOne(sal => sal.Student)
                .WithMany(s => s.StudentActivityLogs)
                .HasForeignKey(sal => sal.StudentID);


            // Configure decimal properties for precision
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentTransaction>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);



            modelBuilder.Entity<Staff>()
                .Property(s => s.Salary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RoomType>()
                .Property(rt => rt.Price)
                .HasPrecision(18, 2);
        }
    }
}
