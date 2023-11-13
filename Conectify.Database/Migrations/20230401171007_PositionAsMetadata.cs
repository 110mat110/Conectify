using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations;

public partial class PositionAsMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Devices_Position_PositionId",
            table: "Devices");

        migrationBuilder.DropForeignKey(
            name: "FK_Rules_Actuators_DestinationActuatorId",
            table: "Rules");

        migrationBuilder.DropForeignKey(
            name: "FK_Rules_Sensors_SourceSensorId",
            table: "Rules");

        migrationBuilder.DropTable(
            name: "Position");

        migrationBuilder.DropIndex(
            name: "IX_Rules_DestinationActuatorId",
            table: "Rules");

        migrationBuilder.DropIndex(
            name: "IX_Rules_SourceSensorId",
            table: "Rules");

        migrationBuilder.DropIndex(
            name: "IX_Devices_PositionId",
            table: "Devices");

        migrationBuilder.DropColumn(
            name: "DestinationActuatorId",
            table: "Rules");

        migrationBuilder.DropColumn(
            name: "SourceSensorId",
            table: "Rules");

        migrationBuilder.DropColumn(
            name: "PositionId",
            table: "Devices");

        migrationBuilder.AddColumn<string>(
            name: "Code",
            table: "Metadatas",
            type: "text",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Code",
            table: "Metadatas");

        migrationBuilder.AddColumn<Guid>(
            name: "DestinationActuatorId",
            table: "Rules",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "SourceSensorId",
            table: "Rules",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "PositionId",
            table: "Devices",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Position",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                Lat = table.Column<float>(type: "real", nullable: false),
                Long = table.Column<float>(type: "real", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Position", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Rules_DestinationActuatorId",
            table: "Rules",
            column: "DestinationActuatorId");

        migrationBuilder.CreateIndex(
            name: "IX_Rules_SourceSensorId",
            table: "Rules",
            column: "SourceSensorId");

        migrationBuilder.CreateIndex(
            name: "IX_Devices_PositionId",
            table: "Devices",
            column: "PositionId");

        migrationBuilder.AddForeignKey(
            name: "FK_Devices_Position_PositionId",
            table: "Devices",
            column: "PositionId",
            principalTable: "Position",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Rules_Actuators_DestinationActuatorId",
            table: "Rules",
            column: "DestinationActuatorId",
            principalTable: "Actuators",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Rules_Sensors_SourceSensorId",
            table: "Rules",
            column: "SourceSensorId",
            principalTable: "Sensors",
            principalColumn: "Id");
    }
}
