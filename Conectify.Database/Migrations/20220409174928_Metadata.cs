using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations
{
    public partial class Metadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UniversalMetadatas_Actuators_ActuatorId",
                table: "UniversalMetadatas");

            migrationBuilder.DropForeignKey(
                name: "FK_UniversalMetadatas_Devices_DeviceId",
                table: "UniversalMetadatas");

            migrationBuilder.DropForeignKey(
                name: "FK_UniversalMetadatas_Sensors_SensorId",
                table: "UniversalMetadatas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UniversalMetadatas",
                table: "UniversalMetadatas");

            migrationBuilder.DropIndex(
                name: "IX_UniversalMetadatas_ActuatorId",
                table: "UniversalMetadatas");

            migrationBuilder.DropIndex(
                name: "IX_UniversalMetadatas_DeviceId",
                table: "UniversalMetadatas");

            migrationBuilder.DropIndex(
                name: "IX_UniversalMetadatas_SensorId",
                table: "UniversalMetadatas");

            migrationBuilder.DropColumn(
                name: "ActuatorId",
                table: "UniversalMetadatas");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "UniversalMetadatas");

            migrationBuilder.DropColumn(
                name: "MaxVal",
                table: "UniversalMetadatas");

            migrationBuilder.DropColumn(
                name: "MinVal",
                table: "UniversalMetadatas");

            migrationBuilder.DropColumn(
                name: "NumericValue",
                table: "UniversalMetadatas");

            migrationBuilder.DropColumn(
                name: "SensorId",
                table: "UniversalMetadatas");

            migrationBuilder.DropColumn(
                name: "StringValue",
                table: "UniversalMetadatas");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "UniversalMetadatas");

            migrationBuilder.RenameTable(
                name: "UniversalMetadatas",
                newName: "Metadatas");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Metadatas",
                table: "Metadatas",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MetadataConnector<Actuator>",
                columns: table => new
                {
                    MetadataId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumericValue = table.Column<float>(type: "real", nullable: true),
                    StringValue = table.Column<string>(type: "text", nullable: false),
                    TypeValue = table.Column<int>(type: "integer", nullable: true),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    MinVal = table.Column<float>(type: "real", nullable: true),
                    MaxVal = table.Column<float>(type: "real", nullable: true),
                    Discriminator = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataConnector<Actuator>", x => new { x.DeviceId, x.MetadataId });
                    table.ForeignKey(
                        name: "FK_MetadataConnector<Actuator>_Actuators_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Actuators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetadataConnector<Actuator>_Metadatas_MetadataId",
                        column: x => x.MetadataId,
                        principalTable: "Metadatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetadataConnector<Device>",
                columns: table => new
                {
                    MetadataId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumericValue = table.Column<float>(type: "real", nullable: true),
                    StringValue = table.Column<string>(type: "text", nullable: false),
                    TypeValue = table.Column<int>(type: "integer", nullable: true),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    MinVal = table.Column<float>(type: "real", nullable: true),
                    MaxVal = table.Column<float>(type: "real", nullable: true),
                    Discriminator = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataConnector<Device>", x => new { x.DeviceId, x.MetadataId });
                    table.ForeignKey(
                        name: "FK_MetadataConnector<Device>_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetadataConnector<Device>_Metadatas_MetadataId",
                        column: x => x.MetadataId,
                        principalTable: "Metadatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetadataConnector<Sensor>",
                columns: table => new
                {
                    MetadataId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumericValue = table.Column<float>(type: "real", nullable: true),
                    StringValue = table.Column<string>(type: "text", nullable: false),
                    TypeValue = table.Column<int>(type: "integer", nullable: true),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    MinVal = table.Column<float>(type: "real", nullable: true),
                    MaxVal = table.Column<float>(type: "real", nullable: true),
                    Discriminator = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataConnector<Sensor>", x => new { x.DeviceId, x.MetadataId });
                    table.ForeignKey(
                        name: "FK_MetadataConnector<Sensor>_Metadatas_MetadataId",
                        column: x => x.MetadataId,
                        principalTable: "Metadatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetadataConnector<Sensor>_Sensors_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetadataConnector<Actuator>_MetadataId",
                table: "MetadataConnector<Actuator>",
                column: "MetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_MetadataConnector<Device>_MetadataId",
                table: "MetadataConnector<Device>",
                column: "MetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_MetadataConnector<Sensor>_MetadataId",
                table: "MetadataConnector<Sensor>",
                column: "MetadataId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetadataConnector<Actuator>");

            migrationBuilder.DropTable(
                name: "MetadataConnector<Device>");

            migrationBuilder.DropTable(
                name: "MetadataConnector<Sensor>");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Metadatas",
                table: "Metadatas");

            migrationBuilder.RenameTable(
                name: "Metadatas",
                newName: "UniversalMetadatas");

            migrationBuilder.AddColumn<Guid>(
                name: "ActuatorId",
                table: "UniversalMetadatas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceId",
                table: "UniversalMetadatas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "MaxVal",
                table: "UniversalMetadatas",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "MinVal",
                table: "UniversalMetadatas",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "NumericValue",
                table: "UniversalMetadatas",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SensorId",
                table: "UniversalMetadatas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StringValue",
                table: "UniversalMetadatas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "UniversalMetadatas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UniversalMetadatas",
                table: "UniversalMetadatas",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UniversalMetadatas_ActuatorId",
                table: "UniversalMetadatas",
                column: "ActuatorId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversalMetadatas_DeviceId",
                table: "UniversalMetadatas",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversalMetadatas_SensorId",
                table: "UniversalMetadatas",
                column: "SensorId");

            migrationBuilder.AddForeignKey(
                name: "FK_UniversalMetadatas_Actuators_ActuatorId",
                table: "UniversalMetadatas",
                column: "ActuatorId",
                principalTable: "Actuators",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UniversalMetadatas_Devices_DeviceId",
                table: "UniversalMetadatas",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UniversalMetadatas_Sensors_SensorId",
                table: "UniversalMetadatas",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id");
        }
    }
}
