using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HostelManagementSystemApi.Migrations
{
    /// <inheritdoc />
    public partial class RefactorRoomAndBedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beds_Users_OccupiedByUserID",
                table: "Beds");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Beds_BedID",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Rooms_RoomID",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_BedID",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Beds_OccupiedByUserID",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "RoomType",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "Rooms",
                newName: "RoomTypeID");

            migrationBuilder.RenameColumn(
                name: "OccupiedByUserID",
                table: "Beds",
                newName: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomTypeID",
                table: "Rooms",
                column: "RoomTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Beds_StudentID",
                table: "Beds",
                column: "StudentID",
                unique: true,
                filter: "[StudentID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Beds_Students_StudentID",
                table: "Beds",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "StudentID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_RoomTypes_RoomTypeID",
                table: "Rooms",
                column: "RoomTypeID",
                principalTable: "RoomTypes",
                principalColumn: "RoomTypeID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Rooms_RoomID",
                table: "Students",
                column: "RoomID",
                principalTable: "Rooms",
                principalColumn: "RoomID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beds_Students_StudentID",
                table: "Beds");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_RoomTypes_RoomTypeID",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Rooms_RoomID",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_RoomTypeID",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Beds_StudentID",
                table: "Beds");

            migrationBuilder.RenameColumn(
                name: "RoomTypeID",
                table: "Rooms",
                newName: "Capacity");

            migrationBuilder.RenameColumn(
                name: "StudentID",
                table: "Beds",
                newName: "OccupiedByUserID");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Rooms",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RoomType",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Students_BedID",
                table: "Students",
                column: "BedID",
                unique: true,
                filter: "[BedID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Beds_OccupiedByUserID",
                table: "Beds",
                column: "OccupiedByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Beds_Users_OccupiedByUserID",
                table: "Beds",
                column: "OccupiedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Beds_BedID",
                table: "Students",
                column: "BedID",
                principalTable: "Beds",
                principalColumn: "BedID");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Rooms_RoomID",
                table: "Students",
                column: "RoomID",
                principalTable: "Rooms",
                principalColumn: "RoomID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
