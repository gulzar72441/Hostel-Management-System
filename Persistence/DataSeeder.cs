using Bogus;
using HostelManagementSystemApi.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HostelManagementSystemApi.Persistence
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (await context.Users.AnyAsync())
            {
                return; // DB has been seeded
            }

            var random = new Random();

            // --- 1. Seed Core Lookups ---
            var roles = new List<Role>
            {
                new Role { RoleName = "Admin" }, new Role { RoleName = "Vendor" },
                new Role { RoleName = "Manager" }, new Role { RoleName = "Warden" },
                new Role { RoleName = "Student" }, new Role { RoleName = "Guardian" }
            };
            await context.Roles.AddRangeAsync(roles);

            var amenities = new List<Amenity>
            {
                new Amenity { Name = "Wi-Fi" }, new Amenity { Name = "Air Conditioning" },
                new Amenity { Name = "Hot Water" }, new Amenity { Name = "Laundry" },
                new Amenity { Name = "CCTV" }, new Amenity { Name = "Parking" },
                new Amenity { Name = "Mess" }, new Amenity { Name = "Gym" }
            };
            await context.Amenities.AddRangeAsync(amenities);

            // --- Get Role References ---
            var adminRole = roles.First(r => r.RoleName == "Admin");
            var vendorRole = roles.First(r => r.RoleName == "Vendor");
            var managerRole = roles.First(r => r.RoleName == "Manager");
            var wardenRole = roles.First(r => r.RoleName == "Warden");
            var studentRole = roles.First(r => r.RoleName == "Student");

            // --- 2. Generate All Entities in Memory ---

            // User Faker
            var userFaker = new Faker<User>()
                .RuleFor(u => u.Name, f => f.Name.FullName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name).ToLower())
                .RuleFor(u => u.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("password123"))
                .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(2));

            // Admin User
            var adminUser = new User
            {
                Name = "Admin User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Email = "admin@hostel.com",
                Phone = "123-456-7890",
                Role = adminRole
            };

            // Vendor Users and Vendors
            var vendorUsers = userFaker.Clone().RuleFor(u => u.Role, vendorRole).Generate(5);
            var vendors = vendorUsers.Select(user => new Vendor { User = user, CompanyName = $"{user.Name} Hostels Inc." }).ToList();

            // Student Users and Students
            var studentUsers = userFaker.Clone().RuleFor(u => u.Role, studentRole).Generate(50);
            var students = studentUsers.Select(u => new Student { User = u }).ToList();

            // All other entities
            var allHostels = new List<Hostel>();
            var allStaff = new List<Staff>();
            var allRoomTypes = new List<RoomType>();
            var allRooms = new List<Room>();
            var allBeds = new List<Bed>();
            var allNotices = new List<Notice>();
            var allBookings = new List<Booking>();
            var allPayments = new List<Payment>();
            var allReviews = new List<HostelReview>();
            var allStaffUsers = new List<User>();

            // Hostel Faker
            var hostelFaker = new Faker<Hostel>()
                .RuleFor(h => h.Name, f => f.Company.CompanyName() + " Hostel")
                .RuleFor(h => h.Address, f => f.Address.StreetAddress())
                .RuleFor(h => h.City, f => f.Address.City())
                .RuleFor(h => h.State, f => f.Address.State())
                .RuleFor(h => h.Country, "India")
                .RuleFor(h => h.Type, f => f.PickRandom(new[] { "Boys", "Girls", "Co-ed" }))
                .RuleFor(h => h.GeoLocation, f => $"{f.Address.Latitude()},{f.Address.Longitude()}")
                .RuleFor(h => h.Description, f => f.Lorem.Paragraph())
                .RuleFor(h => h.IsActive, true)
                .RuleFor(h => h.IsApproved, true)
                .RuleFor(h => h.DateListed, f => f.Date.Past(1));

            foreach (var vendor in vendors)
            {
                var newHostel = hostelFaker.Clone().RuleFor(h => h.Vendor, vendor).Generate();
                newHostel.Amenities = new Faker().PickRandom(amenities, 4).ToList();
                newHostel.HostelImages = new Faker<HostelImage>()
                    .RuleFor(i => i.ImageUrl, f => f.Image.PicsumUrl(600, 400))
                    .RuleFor(i => i.Caption, f => f.Lorem.Sentence(5))
                    .RuleFor(i => i.IsPrimary, (f, i) => f.Random.Bool(0.8f))
                    .Generate(5);
                newHostel.HostelImages.First().IsPrimary = true;
                allHostels.Add(newHostel);

                // Staff
                var managerUser = userFaker.Clone().RuleFor(u => u.Role, managerRole).Generate();
                var wardenUser = userFaker.Clone().RuleFor(u => u.Role, wardenRole).Generate();
                allStaffUsers.Add(managerUser);
                allStaffUsers.Add(wardenUser);
                allStaff.Add(new Staff { User = managerUser, Hostel = newHostel, Salary = 70000, JoinDate = DateTime.UtcNow.AddMonths(-6) });
                allStaff.Add(new Staff { User = wardenUser, Hostel = newHostel, Salary = 45000, JoinDate = DateTime.UtcNow.AddMonths(-3) });

                // Room Types, Rooms, and Beds
                var roomTypeData = new List<(string Name, int Capacity, decimal Price)>
                {
                    ("Single", 1, 12000m), ("Double Sharing", 2, 8000m), ("Triple Sharing", 3, 6000m)
                };

                foreach (var rtData in roomTypeData)
                {
                    var roomType = new RoomType { Hostel = newHostel, Name = rtData.Name, Capacity = rtData.Capacity, Price = rtData.Price };
                    allRoomTypes.Add(roomType);

                    for (int i = 1; i <= 5; i++)
                    {
                        var room = new Room { Hostel = newHostel, RoomType = roomType, RoomNumber = $"{rtData.Name.First()}{i}", Description = $"A cozy {rtData.Name.ToLower()} room." };
                        allRooms.Add(room);

                        for (int j = 1; j <= rtData.Capacity; j++)
                        {
                            allBeds.Add(new Bed { Room = room, BedNumber = $"{room.RoomNumber}-B{j}" });
                        }
                    }
                }

                // Notices
                allNotices.AddRange(new Faker<Notice>()
                    .RuleFor(n => n.Hostel, newHostel)
                    .RuleFor(n => n.Title, f => f.Lorem.Sentence(4))
                    .RuleFor(n => n.Content, f => f.Lorem.Paragraphs(2))
                    .RuleFor(n => n.Date, f => f.Date.Past(1))
                    .Generate(3));
            }

            // Assign every student to a random hostel to satisfy FK constraint
            foreach (var student in students)
            {
                student.Hostel = allHostels[random.Next(allHostels.Count)];
            }

            // Student Interactions
            var availableBeds = new List<Bed>(allBeds);
            foreach (var student in students.Take(30))
            {
                var bedsInStudentHostel = availableBeds.Where(b => b.Room.Hostel == student.Hostel && !b.IsOccupied).ToList();
                if (!bedsInStudentHostel.Any()) continue;

                var bed = bedsInStudentHostel[random.Next(bedsInStudentHostel.Count)];
                bed.IsOccupied = true;
                bed.Student = student;
                availableBeds.Remove(bed);

                var booking = new Booking
                {
                    Student = student,
                    Room = bed.Room,
                    BookingDate = DateTime.UtcNow.AddMonths(-2),
                    CheckInDate = DateTime.UtcNow.AddMonths(-2),
                    CheckOutDate = DateTime.UtcNow.AddMonths(10),
                    Status = "Confirmed",
                    TotalPrice = bed.Room.RoomType.Price * 12
                };
                allBookings.Add(booking);

                allPayments.Add(new Payment
                {
                    Student = student,
                    Hostel = bed.Room.Hostel,
                    Amount = bed.Room.RoomType.Price,
                    DueDate = DateTime.UtcNow.AddDays(5),
                    PaidDate = DateTime.UtcNow.AddDays(-1),
                    Status = "Paid",
                    Method = "Online"
                });

                allReviews.Add(new Faker<HostelReview>()
                    .RuleFor(r => r.Hostel, bed.Room.Hostel)
                    .RuleFor(r => r.Student, student)
                    .RuleFor(r => r.Rating, f => f.Random.Int(3, 5))
                    .RuleFor(r => r.Comment, f => f.Lorem.Sentence())
                    .RuleFor(r => r.Date, f => f.Date.Past())
                    .Generate());
            }

            // --- 3. Add all generated entities to the context ---
            await context.Users.AddAsync(adminUser);
            await context.Users.AddRangeAsync(vendorUsers);
            await context.Users.AddRangeAsync(studentUsers);
            await context.Users.AddRangeAsync(allStaffUsers);
            await context.Vendors.AddRangeAsync(vendors);
            await context.Students.AddRangeAsync(students);
            await context.Hostels.AddRangeAsync(allHostels);
            await context.Staff.AddRangeAsync(allStaff);
            await context.RoomTypes.AddRangeAsync(allRoomTypes);
            await context.Rooms.AddRangeAsync(allRooms);
            await context.Beds.AddRangeAsync(allBeds);
            await context.Notices.AddRangeAsync(allNotices);
            await context.Bookings.AddRangeAsync(allBookings);
            await context.Payments.AddRangeAsync(allPayments);
            await context.HostelReviews.AddRangeAsync(allReviews);

            // --- Additional Entities ---

            var allUsers = vendorUsers.Concat(studentUsers).Concat(allStaffUsers).Append(adminUser).ToList();

            // Seed System Logs
            var systemLogFaker = new Faker<SystemLog>()
                .RuleFor(l => l.LogLevel, f => f.PickRandom(new[] { "Info", "Warning", "Error" }))
                .RuleFor(l => l.Message, f => f.Lorem.Sentence())
                .RuleFor(l => l.Timestamp, f => f.Date.Past(1));

            var systemLogs = systemLogFaker.Generate(100);
            await context.SystemLogs.AddRangeAsync(systemLogs);

            // Seed System Settings
            var systemSettings = new List<SystemSetting>
            {
                new SystemSetting { SettingKey = "SiteName", SettingValue = "Hostel Management Pro" },
                new SystemSetting { SettingKey = "MaintenanceMode", SettingValue = "false" },
                new SystemSetting { SettingKey = "DefaultLanguage", SettingValue = "en-US" }
            };
            await context.SystemSettings.AddRangeAsync(systemSettings);

            // Seed Attendances
            var attendanceFaker = new Faker<Attendance>()
                .RuleFor(a => a.Student, f => f.PickRandom(students))
                .RuleFor(a => a.Date, f => f.Date.Past(1, DateTime.Now.AddDays(-1)))
                .RuleFor(a => a.Status, f => f.PickRandom(new[] { "Present", "Absent", "On-Leave" }))
                .RuleFor(a => a.CheckInTime, (f, a) => a.Status == "Present" ? a.Date.AddHours(f.Random.Int(7, 9)) : (DateTime?)null)
                .RuleFor(a => a.CheckOutTime, (f, a) => a.Status == "Present" ? a.Date.AddHours(f.Random.Int(18, 20)) : (DateTime?)null);

            var attendances = attendanceFaker.Generate(200);
            await context.Attendances.AddRangeAsync(attendances);

            // Seed Attendance Reasons for 'On-Leave' statuses
            var onLeaveAttendances = attendances.Where(a => a.Status == "On-Leave").ToList();
            var attendanceReasonFaker = new Faker<AttendanceReason>()
                .RuleFor(ar => ar.Attendance, f => f.PickRandom(onLeaveAttendances))
                .RuleFor(ar => ar.Reason, f => f.Lorem.Sentence(5));

            var attendanceReasons = attendanceReasonFaker.Generate(onLeaveAttendances.Count);
            await context.AttendanceReasons.AddRangeAsync(attendanceReasons);

            // Seed Chat Messages
            var chatMessageFaker = new Faker<ChatMessage>()
                .RuleFor(cm => cm.Sender, f => f.PickRandom(allUsers))
                .RuleFor(cm => cm.Recipient, f => f.PickRandom(allUsers))
                .RuleFor(cm => cm.Content, f => f.Lorem.Sentence())
                .RuleFor(cm => cm.Timestamp, f => f.Date.Past(1));

            var chatMessages = chatMessageFaker.Generate(500);
            await context.ChatMessages.AddRangeAsync(chatMessages);

            // Seed Complaints
            var complaintFaker = new Faker<Complaint>()
                .RuleFor(c => c.Student, f => f.PickRandom(students))
                .RuleFor(c => c.Hostel, (f, c) => f.PickRandom(allHostels))
                .RuleFor(c => c.Type, f => f.PickRandom(new[] { "Maintenance", "Food", "Security", "Staff" }))
                .RuleFor(c => c.Description, f => f.Lorem.Paragraph())
                .RuleFor(c => c.Status, f => f.PickRandom(new[] { "Pending", "In-Progress", "Resolved" }))
                .RuleFor(c => c.CreatedAt, f => f.Date.Past(1));

            var complaints = complaintFaker.Generate(30);
            await context.Complaints.AddRangeAsync(complaints);

            // Seed Guardians
            var guardianFaker = new Faker<Guardian>()
                .RuleFor(g => g.Student, f => f.PickRandom(students))
                .RuleFor(g => g.Name, (f, g) => f.Name.FullName())
                .RuleFor(g => g.Phone, (f, g) => f.Phone.PhoneNumber())
                .RuleFor(g => g.Relation, f => f.PickRandom(new[] { "Father", "Mother", "Guardian" }));

            var guardians = guardianFaker.Generate(students.Count);
            await context.Guardians.AddRangeAsync(guardians);

            // Seed Inventories
            var inventoryFaker = new Faker<Inventory>()
                .RuleFor(i => i.Hostel, f => f.PickRandom(allHostels))
                .RuleFor(i => i.ItemName, f => f.Commerce.ProductName())
                .RuleFor(i => i.Quantity, f => f.Random.Int(10, 100))
                .RuleFor(i => i.Unit, f => f.PickRandom(new[] { "pcs", "kg", "liters" }));

            var inventories = inventoryFaker.Generate(50);
            await context.Inventories.AddRangeAsync(inventories);

            // Seed Inventory Logs
            var inventoryLogFaker = new Faker<InventoryLog>()
                .RuleFor(il => il.Inventory, f => f.PickRandom(inventories))
                .RuleFor(il => il.Staff, f => f.PickRandom(allStaff))
                .RuleFor(il => il.Action, f => f.PickRandom(new[] { "Added", "Used", "Removed" }))
                .RuleFor(il => il.QuantityChanged, f => f.Random.Int(1, 10))
                .RuleFor(il => il.Date, f => f.Date.Past(1));

            var inventoryLogs = inventoryLogFaker.Generate(100);
            await context.InventoryLogs.AddRangeAsync(inventoryLogs);

            // Seed Notification Preferences
            var notificationPreferenceFaker = new Faker<NotificationPreference>()
                .RuleFor(np => np.User, f => f.PickRandom(allUsers))
                .RuleFor(np => np.NotificationType, f => f.PickRandom(new[] { "Fee-Reminder", "Notice", "Complaint-Update" }))
                .RuleFor(np => np.IsEmailEnabled, f => f.Random.Bool())
                .RuleFor(np => np.IsSMSEnabled, f => f.Random.Bool())
                .RuleFor(np => np.IsPushEnabled, f => f.Random.Bool());

            var notificationPreferences = notificationPreferenceFaker.Generate(allUsers.Count * 2);
            await context.NotificationPreferences.AddRangeAsync(notificationPreferences);

            // Seed Payment Transactions
            var paymentTransactionFaker = new Faker<PaymentTransaction>()
                .RuleFor(pt => pt.Payment, f => f.PickRandom(allPayments))
                .RuleFor(pt => pt.TransactionType, f => f.PickRandom(new[] { "Payment", "Refund" }))
                .RuleFor(pt => pt.Amount, (f, pt) => pt.TransactionType == "Refund" ? f.Random.Decimal(100, 500) : pt.Payment.Amount)
                .RuleFor(pt => pt.TransactionDate, f => f.Date.Past(1))
                .RuleFor(pt => pt.GatewayTransactionID, f => f.Random.Guid().ToString());

            var paymentTransactions = paymentTransactionFaker.Generate(allPayments.Count);
            await context.PaymentTransactions.AddRangeAsync(paymentTransactions);

            // Seed Staff Attendances
            var staffAttendanceFaker = new Faker<StaffAttendance>()
                .RuleFor(sa => sa.Staff, f => f.PickRandom(allStaff))
                .RuleFor(sa => sa.Date, f => f.Date.Past(1, DateTime.Now.AddDays(-1)))
                .RuleFor(sa => sa.Status, f => f.PickRandom(new[] { "Present", "Absent", "On-Leave" }))
                .RuleFor(sa => sa.CheckInTime, (f, sa) => sa.Status == "Present" ? sa.Date.AddHours(f.Random.Int(8, 10)) : (DateTime?)null)
                .RuleFor(sa => sa.CheckOutTime, (f, sa) => sa.Status == "Present" ? sa.Date.AddHours(f.Random.Int(17, 19)) : (DateTime?)null);

            var staffAttendances = staffAttendanceFaker.Generate(150);
            await context.StaffAttendances.AddRangeAsync(staffAttendances);

            // Seed Student Activity Logs
            var studentActivityLogFaker = new Faker<StudentActivityLog>()
                .RuleFor(sl => sl.Student, f => f.PickRandom(students))
                .RuleFor(sl => sl.ActivityType, f => f.PickRandom(new[] { "Login", "Logout", "Fee-Payment", "Profile-Update" }))
                .RuleFor(sl => sl.Timestamp, f => f.Date.Past(1));

            var studentActivityLogs = studentActivityLogFaker.Generate(300);
            await context.StudentActivityLogs.AddRangeAsync(studentActivityLogs);

            // --- 4. Save everything in a single transaction ---
            await context.SaveChangesAsync();
        }
    }
}