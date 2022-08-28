using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations
{
    public partial class Rules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetadataConnector<Actuator>_Actuators_DeviceId",
                table: "MetadataConnector<Actuator>");

            migrationBuilder.DropForeignKey(
                name: "FK_MetadataConnector<Actuator>_Metadatas_MetadataId",
                table: "MetadataConnector<Actuator>");

            migrationBuilder.DropForeignKey(
                name: "FK_MetadataConnector<Device>_Devices_DeviceId",
                table: "MetadataConnector<Device>");

            migrationBuilder.DropForeignKey(
                name: "FK_MetadataConnector<Device>_Metadatas_MetadataId",
                table: "MetadataConnector<Device>");

            migrationBuilder.DropForeignKey(
                name: "FK_MetadataConnector<Sensor>_Metadatas_MetadataId",
                table: "MetadataConnector<Sensor>");

            migrationBuilder.DropForeignKey(
                name: "FK_MetadataConnector<Sensor>_Sensors_DeviceId",
                table: "MetadataConnector<Sensor>");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetadataConnector<Sensor>",
                table: "MetadataConnector<Sensor>");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetadataConnector<Device>",
                table: "MetadataConnector<Device>");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetadataConnector<Actuator>",
                table: "MetadataConnector<Actuator>");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "MetadataConnector<Sensor>");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "MetadataConnector<Device>");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "MetadataConnector<Actuator>");

            migrationBuilder.RenameTable(
                name: "MetadataConnector<Sensor>",
                newName: "SensorMetadata");

            migrationBuilder.RenameTable(
                name: "MetadataConnector<Device>",
                newName: "DeviceMetadata");

            migrationBuilder.RenameTable(
                name: "MetadataConnector<Actuator>",
                newName: "ActuatorMetadatas");

            migrationBuilder.RenameIndex(
                name: "IX_MetadataConnector<Sensor>_MetadataId",
                table: "SensorMetadata",
                newName: "IX_SensorMetadata_MetadataId");

            migrationBuilder.RenameIndex(
                name: "IX_MetadataConnector<Device>_MetadataId",
                table: "DeviceMetadata",
                newName: "IX_DeviceMetadata_MetadataId");

            migrationBuilder.RenameIndex(
                name: "IX_MetadataConnector<Actuator>_MetadataId",
                table: "ActuatorMetadatas",
                newName: "IX_ActuatorMetadatas_MetadataId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorMetadata",
                table: "SensorMetadata",
                columns: new[] { "DeviceId", "MetadataId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceMetadata",
                table: "DeviceMetadata",
                columns: new[] { "DeviceId", "MetadataId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActuatorMetadatas",
                table: "ActuatorMetadatas",
                columns: new[] { "DeviceId", "MetadataId" });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ParametersJson = table.Column<string>(type: "text", nullable: false),
                    RuleType = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSensorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationActuatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rules_Actuators_DestinationActuatorId",
                        column: x => x.DestinationActuatorId,
                        principalTable: "Actuators",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rules_Sensors_SourceSensorId",
                        column: x => x.SourceSensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RuleConnector",
                columns: table => new
                {
                    ContinuingRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousRuleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleConnector", x => new { x.PreviousRuleId, x.ContinuingRuleId });
                    table.ForeignKey(
                        name: "FK_RuleConnector_Rules_ContinuingRuleId",
                        column: x => x.ContinuingRuleId,
                        principalTable: "Rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RuleConnector_Rules_PreviousRuleId",
                        column: x => x.PreviousRuleId,
                        principalTable: "Rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RuleConnector_ContinuingRuleId",
                table: "RuleConnector",
                column: "ContinuingRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Rules_DestinationActuatorId",
                table: "Rules",
                column: "DestinationActuatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Rules_SourceSensorId",
                table: "Rules",
                column: "SourceSensorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActuatorMetadatas_Actuators_DeviceId",
                table: "ActuatorMetadatas",
                column: "DeviceId",
                principalTable: "Actuators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActuatorMetadatas_Metadatas_MetadataId",
                table: "ActuatorMetadatas",
                column: "MetadataId",
                principalTable: "Metadatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceMetadata_Devices_DeviceId",
                table: "DeviceMetadata",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceMetadata_Metadatas_MetadataId",
                table: "DeviceMetadata",
                column: "MetadataId",
                principalTable: "Metadatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorMetadata_Metadatas_MetadataId",
                table: "SensorMetadata",
                column: "MetadataId",
                principalTable: "Metadatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorMetadata_Sensors_DeviceId",
                table: "SensorMetadata",
                column: "DeviceId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActuatorMetadatas_Actuators_DeviceId",
                table: "ActuatorMetadatas");

            migrationBuilder.DropForeignKey(
                name: "FK_ActuatorMetadatas_Metadatas_MetadataId",
                table: "ActuatorMetadatas");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceMetadata_Devices_DeviceId",
                table: "DeviceMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceMetadata_Metadatas_MetadataId",
                table: "DeviceMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorMetadata_Metadatas_MetadataId",
                table: "SensorMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorMetadata_Sensors_DeviceId",
                table: "SensorMetadata");

            migrationBuilder.DropTable(
                name: "RuleConnector");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorMetadata",
                table: "SensorMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceMetadata",
                table: "DeviceMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActuatorMetadatas",
                table: "ActuatorMetadatas");

            migrationBuilder.RenameTable(
                name: "SensorMetadata",
                newName: "MetadataConnector<Sensor>");

            migrationBuilder.RenameTable(
                name: "DeviceMetadata",
                newName: "MetadataConnector<Device>");

            migrationBuilder.RenameTable(
                name: "ActuatorMetadatas",
                newName: "MetadataConnector<Actuator>");

            migrationBuilder.RenameIndex(
                name: "IX_SensorMetadata_MetadataId",
                table: "MetadataConnector<Sensor>",
                newName: "IX_MetadataConnector<Sensor>_MetadataId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceMetadata_MetadataId",
                table: "MetadataConnector<Device>",
                newName: "IX_MetadataConnector<Device>_MetadataId");

            migrationBuilder.RenameIndex(
                name: "IX_ActuatorMetadatas_MetadataId",
                table: "MetadataConnector<Actuator>",
                newName: "IX_MetadataConnector<Actuator>_MetadataId");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "MetadataConnector<Sensor>",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "MetadataConnector<Device>",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "MetadataConnector<Actuator>",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetadataConnector<Sensor>",
                table: "MetadataConnector<Sensor>",
                columns: new[] { "DeviceId", "MetadataId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetadataConnector<Device>",
                table: "MetadataConnector<Device>",
                columns: new[] { "DeviceId", "MetadataId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetadataConnector<Actuator>",
                table: "MetadataConnector<Actuator>",
                columns: new[] { "DeviceId", "MetadataId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MetadataConnector<Actuator>_Actuators_DeviceId",
                table: "MetadataConnector<Actuator>",
                column: "DeviceId",
                principalTable: "Actuators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetadataConnector<Actuator>_Metadatas_MetadataId",
                table: "MetadataConnector<Actuator>",
                column: "MetadataId",
                principalTable: "Metadatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetadataConnector<Device>_Devices_DeviceId",
                table: "MetadataConnector<Device>",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetadataConnector<Device>_Metadatas_MetadataId",
                table: "MetadataConnector<Device>",
                column: "MetadataId",
                principalTable: "Metadatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetadataConnector<Sensor>_Metadatas_MetadataId",
                table: "MetadataConnector<Sensor>",
                column: "MetadataId",
                principalTable: "Metadatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetadataConnector<Sensor>_Sensors_DeviceId",
                table: "MetadataConnector<Sensor>",
                column: "DeviceId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
