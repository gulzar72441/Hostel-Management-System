# 🏨 Hostel Management System API

A comprehensive and robust backend API for a modern Hostel Management System, built with the latest **.NET 9** and C#. This API provides a wide range of features for managing hostels, students, staff, bookings, payments, and much more. It's designed to be scalable, secure, and easy to use.

## ✨ Features

*   👤 **User Management:** Role-based access control (Admin, Vendor, Staff, Student) with JWT authentication.
*   🏢 **Hostel Management:** Create, update, and manage hostel details, including amenities, images, and room types.
*   🛏️ **Room & Bed Management:** Manage rooms and individual beds within a hostel, including availability.
*   📅 **Booking System:** Allow students to book rooms and manage their reservations.
*   💳 **Payment Processing:** Handle payments for bookings and other hostel services.
*   ✅ **Attendance Tracking:** Track student and staff attendance with check-in/check-out times.
*   📝 **Complaint System:** Allow students to raise and track complaints.
*   📢 **Notice Board:** Post and view notices for different hostel audiences.
*   📦 **Inventory Management:** Track and manage hostel inventory and logs.
*   💬 **Real-time Chat:** Direct messaging functionality between users.
*   🗺️ **Public Search:** An open endpoint to search for available rooms by address, city, or state.
*   🌱 **Data Seeding:** Comes with a comprehensive data seeder for easy testing and development.

## 🛠️ Technologies Used

*   🚀 **.NET 9:** The core framework for building the API.
*   💻 **C#:** The primary programming language.
*   🌐 **ASP.NET Core:** For building high-performance web APIs.
*   ⚡ **FastEndpoints:** A lightweight and high-performance alternative to MVC for building API endpoints.
*   🗃️ **Entity Framework Core:** The ORM for database interaction.
*   💾 **SQL Server:** The database provider.
*   🎲 **Bogus:** For generating realistic fake data for seeding.
*   🔒 **BCrypt.Net:** For hashing and securing user passwords.
*   🔑 **JWT (JSON Web Tokens):** For securing endpoints and handling authentication.
*   📖 **Swashbuckle:** For generating beautiful Swagger/OpenAPI documentation.

## 🚀 Getting Started

### Prerequisites

*   [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
*   [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (e.g., Express or Developer edition)

### ⚙️ Installation

1.  **Clone the repository:**
    ```bash
    git clone <repository-url>
    cd HostelManagementSystemApi
    ```
2.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```

### 🗄️ Database Setup

1.  **Update Connection String:** Open `appsettings.json` and update the `DefaultConnection` string to point to your local SQL Server instance.
2.  **Apply Migrations:** Run the following command to create the database and apply the schema.
    ```bash
    dotnet ef database update
    ```
    This will create a database named `HostelManagementDb_FastEndPoint` by default.

### ▶️ Running the Application

1.  Run the project using the .NET CLI:
    ```bash
    dotnet run
    ```
2.  The API will start and be accessible at `http://localhost:5055` (or as configured).
3.  The first time you run the application, the database will be seeded with extensive fake data.

## 🧪 API Endpoints & Testing

A Postman collection and environment file are provided in the root of the project to help you test the API.

*   `Hostel_Management_API.postman_collection.json`
*   `Hostel_Management_API.postman_environment.json`

### How to use:

1.  Import both files into Postman.
2.  Select the `Hostel Management API Environment` in Postman.
3.  Run the `Login to the application` request from the `Auth` folder to get a JWT token. The default admin credentials are:
    *   **Email:** `admin@hostel.com`
    *   **Password:** `admin123`
4.  Copy the `token` from the response and set it as the value for the `jwt_token` environment variable in Postman.
5.  You can now make authenticated requests to the protected endpoints! 🎉

## 🌱 Data Seeding

The project includes a `DataSeeder` that populates the database with a large amount of realistic fake data on the first run. This includes:

*   Users (Admins, Vendors, Staff, Students)
*   Hostels, Rooms, and Beds
*   Bookings, Payments, and Reviews
*   Attendances, Complaints, and Notices
*   And many other system-related records.

If you want to re-seed the database, you can drop it and re-run the application:

```bash
dotnet ef database drop --force
dotnet run
```
