using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace smarthomeserver.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeProcessor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProcessorId",
                table: "SmartHome",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Processors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MqttClientId = table.Column<int>(type: "int", nullable: false),
                    OnlineStatus = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AddedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Processors_MqttClients_MqttClientId",
                        column: x => x.MqttClientId,
                        principalTable: "MqttClients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SmartHome_ProcessorId",
                table: "SmartHome",
                column: "ProcessorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Processors_MqttClientId",
                table: "Processors",
                column: "MqttClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_SmartHome_Processors_ProcessorId",
                table: "SmartHome",
                column: "ProcessorId",
                principalTable: "Processors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmartHome_Processors_ProcessorId",
                table: "SmartHome");

            migrationBuilder.DropTable(
                name: "Processors");

            migrationBuilder.DropIndex(
                name: "IX_SmartHome_ProcessorId",
                table: "SmartHome");

            migrationBuilder.DropColumn(
                name: "ProcessorId",
                table: "SmartHome");
        }
    }
}
