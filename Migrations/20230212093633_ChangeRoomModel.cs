using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace smarthomeserver.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRoomModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_Name_FloorId_HomeId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "HomeId",
                table: "Rooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Name_FloorId",
                table: "Rooms",
                columns: new[] { "Name", "FloorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_Name_FloorId",
                table: "Rooms");

            migrationBuilder.AddColumn<Guid>(
                name: "HomeId",
                table: "Rooms",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Name_FloorId_HomeId",
                table: "Rooms",
                columns: new[] { "Name", "FloorId", "HomeId" },
                unique: true);
        }
    }
}
