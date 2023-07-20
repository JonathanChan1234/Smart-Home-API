using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace smarthomeserver.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSceneAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "actions");

            migrationBuilder.DropColumn(
                name: "Brightness",
                table: "actions");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "actions");

            migrationBuilder.DropColumn(
                name: "action_type",
                table: "actions");

            migrationBuilder.AddColumn<byte[]>(
                name: "Action",
                table: "actions",
                type: "BLOB",
                nullable: false);

            migrationBuilder.AddForeignKey(
                name: "FK_actions_SmartDevices_DeviceId",
                table: "actions",
                column: "DeviceId",
                principalTable: "SmartDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_actions_SmartDevices_DeviceId",
                table: "actions");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "actions");

            migrationBuilder.AddColumn<int>(
                name: "ActionType",
                table: "actions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Brightness",
                table: "actions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "actions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "action_type",
                table: "actions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
