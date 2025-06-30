using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HostelManagementSystemApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAmenities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amenities",
                table: "Hostels");

            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    AmenityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.AmenityID);
                });

            migrationBuilder.CreateTable(
                name: "AmenityHostel",
                columns: table => new
                {
                    AmenitiesAmenityID = table.Column<int>(type: "int", nullable: false),
                    HostelsHostelID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmenityHostel", x => new { x.AmenitiesAmenityID, x.HostelsHostelID });
                    table.ForeignKey(
                        name: "FK_AmenityHostel_Amenities_AmenitiesAmenityID",
                        column: x => x.AmenitiesAmenityID,
                        principalTable: "Amenities",
                        principalColumn: "AmenityID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AmenityHostel_Hostels_HostelsHostelID",
                        column: x => x.HostelsHostelID,
                        principalTable: "Hostels",
                        principalColumn: "HostelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmenityHostel_HostelsHostelID",
                table: "AmenityHostel",
                column: "HostelsHostelID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmenityHostel");

            migrationBuilder.DropTable(
                name: "Amenities");

            migrationBuilder.AddColumn<string>(
                name: "Amenities",
                table: "Hostels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
