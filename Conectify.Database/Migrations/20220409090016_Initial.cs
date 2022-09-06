using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IPAdress = table.Column<string>(type: "text", nullable: false),
                    MacAdress = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SubscibeToAll = table.Column<bool>(type: "boolean", nullable: false),
                    IsKnown = table.Column<bool>(type: "boolean", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Position_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Position",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    StringValue = table.Column<string>(type: "text", nullable: false),
                    NumericValue = table.Column<float>(type: "real", nullable: true),
                    TimeCreated = table.Column<long>(type: "bigint", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commands_Devices_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Commands_Devices_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SourceDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsKnown = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sensors_Devices_SourceDeviceId",
                        column: x => x.SourceDeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommandResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommandId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    StringValue = table.Column<string>(type: "text", nullable: false),
                    NumericValue = table.Column<float>(type: "real", nullable: true),
                    TimeCreated = table.Column<long>(type: "bigint", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommandResponses_Commands_CommandId",
                        column: x => x.CommandId,
                        principalTable: "Commands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommandResponses_Devices_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Actuators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SourceDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SensorId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsKnown = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actuators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actuators_Devices_SourceDeviceId",
                        column: x => x.SourceDeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Actuators_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    StringValue = table.Column<string>(type: "text", nullable: false),
                    NumericValue = table.Column<float>(type: "real", nullable: true),
                    TimeCreated = table.Column<long>(type: "bigint", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Values_Sensors_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    StringValue = table.Column<string>(type: "text", nullable: false),
                    NumericValue = table.Column<float>(type: "real", nullable: true),
                    TimeCreated = table.Column<long>(type: "bigint", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actions_Actuators_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Actuators",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Actions_Sensors_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Preference",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriberId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActuatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    SensorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubToValues = table.Column<bool>(type: "boolean", nullable: false),
                    SubToActions = table.Column<bool>(type: "boolean", nullable: false),
                    SubToCommands = table.Column<bool>(type: "boolean", nullable: false),
                    SubToActionResponse = table.Column<bool>(type: "boolean", nullable: false),
                    SubToCommandResponse = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Preference_Actuators_ActuatorId",
                        column: x => x.ActuatorId,
                        principalTable: "Actuators",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Preference_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Preference_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UniversalMetadatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NumericValue = table.Column<float>(type: "real", nullable: true),
                    StringValue = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    MinVal = table.Column<float>(type: "real", nullable: true),
                    MaxVal = table.Column<float>(type: "real", nullable: true),
                    ActuatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    SensorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversalMetadatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversalMetadatas_Actuators_ActuatorId",
                        column: x => x.ActuatorId,
                        principalTable: "Actuators",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UniversalMetadatas_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UniversalMetadatas_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ActionResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    StringValue = table.Column<string>(type: "text", nullable: false),
                    NumericValue = table.Column<float>(type: "real", nullable: true),
                    TimeCreated = table.Column<long>(type: "bigint", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionResponses_Actions_ActionId1",
                        column: x => x.ActionId1,
                        principalTable: "Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionResponses_Actuators_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Actuators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionResponses_ActionId1",
                table: "ActionResponses",
                column: "ActionId1");

            migrationBuilder.CreateIndex(
                name: "IX_ActionResponses_SourceId",
                table: "ActionResponses",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_DestinationId",
                table: "Actions",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_SourceId",
                table: "Actions",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Actuators_SensorId",
                table: "Actuators",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_Actuators_SourceDeviceId",
                table: "Actuators",
                column: "SourceDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_CommandResponses_CommandId",
                table: "CommandResponses",
                column: "CommandId");

            migrationBuilder.CreateIndex(
                name: "IX_CommandResponses_SourceId",
                table: "CommandResponses",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Commands_DestinationId",
                table: "Commands",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Commands_SourceId",
                table: "Commands",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_PositionId",
                table: "Devices",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Preference_ActuatorId",
                table: "Preference",
                column: "ActuatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Preference_DeviceId",
                table: "Preference",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Preference_SensorId",
                table: "Preference",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_SourceDeviceId",
                table: "Sensors",
                column: "SourceDeviceId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Values_SourceId",
                table: "Values",
                column: "SourceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionResponses");

            migrationBuilder.DropTable(
                name: "CommandResponses");

            migrationBuilder.DropTable(
                name: "Preference");

            migrationBuilder.DropTable(
                name: "UniversalMetadatas");

            migrationBuilder.DropTable(
                name: "Values");

            migrationBuilder.DropTable(
                name: "Actions");

            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "Actuators");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Position");
        }
    }
}
