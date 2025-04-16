using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations;

/// <inheritdoc />
public partial class initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DashboardsDevice",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DashBoardId = table.Column<Guid>(type: "uuid", nullable: false),
                DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                PosX = table.Column<int>(type: "integer", nullable: false),
                PosY = table.Column<int>(type: "integer", nullable: false),
                SourceType = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DashboardsDevice", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Devices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                IPAdress = table.Column<string>(type: "text", nullable: false),
                MacAdress = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                SubscribeToAll = table.Column<bool>(type: "boolean", nullable: false),
                IsKnown = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Devices", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Events",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                DestinationId = table.Column<Guid>(type: "uuid", nullable: true),
                Type = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Unit = table.Column<string>(type: "text", nullable: false),
                StringValue = table.Column<string>(type: "text", nullable: false),
                NumericValue = table.Column<float>(type: "real", nullable: true),
                TimeCreated = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Events", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Metadatas",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Code = table.Column<string>(type: "text", nullable: false),
                Exclusive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Metadatas", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Rules",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                ParametersJson = table.Column<string>(type: "text", nullable: false),
                RuleType = table.Column<Guid>(type: "uuid", nullable: false),
                X = table.Column<int>(type: "integer", nullable: false),
                Y = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rules", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Softwares",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Softwares", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserMail = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Preference",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SubscriberId = table.Column<Guid>(type: "uuid", nullable: false),
                SubscibeeId = table.Column<Guid>(type: "uuid", nullable: true),
                EventType = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Preference", x => x.Id);
                table.ForeignKey(
                    name: "FK_Preference_Devices_SubscriberId",
                    column: x => x.SubscriberId,
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
            name: "DeviceMetadata",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                NumericValue = table.Column<float>(type: "real", nullable: true),
                StringValue = table.Column<string>(type: "text", nullable: false),
                TypeValue = table.Column<int>(type: "integer", nullable: true),
                Unit = table.Column<string>(type: "text", nullable: false),
                MinVal = table.Column<float>(type: "real", nullable: true),
                MaxVal = table.Column<float>(type: "real", nullable: true),
                MetadataId = table.Column<Guid>(type: "uuid", nullable: false),
                DeviceId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DeviceMetadata", x => x.Id);
                table.ForeignKey(
                    name: "FK_DeviceMetadata_Devices_DeviceId",
                    column: x => x.DeviceId,
                    principalTable: "Devices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DeviceMetadata_Metadatas_MetadataId",
                    column: x => x.MetadataId,
                    principalTable: "Metadatas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
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

        migrationBuilder.CreateTable(
            name: "RuleParameter",
            columns: table => new
            {
                SourceRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                TargetRuleId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RuleParameter", x => new { x.SourceRuleId, x.TargetRuleId });
                table.ForeignKey(
                    name: "FK_RuleParameter_Rules_SourceRuleId",
                    column: x => x.SourceRuleId,
                    principalTable: "Rules",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RuleParameter_Rules_TargetRuleId",
                    column: x => x.TargetRuleId,
                    principalTable: "Rules",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DeviceVersions",
            columns: table => new
            {
                DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                SoftwareId = table.Column<Guid>(type: "uuid", nullable: false),
                LastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ChipVersion = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DeviceVersions", x => x.DeviceId);
                table.ForeignKey(
                    name: "FK_DeviceVersions_Devices_DeviceId",
                    column: x => x.DeviceId,
                    principalTable: "Devices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DeviceVersions_Softwares_SoftwareId",
                    column: x => x.SoftwareId,
                    principalTable: "Softwares",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SoftwareVersions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SoftwareId = table.Column<Guid>(type: "uuid", nullable: false),
                ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Url = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SoftwareVersions", x => x.Id);
                table.ForeignKey(
                    name: "FK_SoftwareVersions_Softwares_SoftwareId",
                    column: x => x.SoftwareId,
                    principalTable: "Softwares",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Dashboards",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Position = table.Column<int>(type: "integer", nullable: false),
                Background = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Dashboards", x => x.Id);
                table.ForeignKey(
                    name: "FK_Dashboards_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
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
            name: "SensorMetadata",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                NumericValue = table.Column<float>(type: "real", nullable: true),
                StringValue = table.Column<string>(type: "text", nullable: false),
                TypeValue = table.Column<int>(type: "integer", nullable: true),
                Unit = table.Column<string>(type: "text", nullable: false),
                MinVal = table.Column<float>(type: "real", nullable: true),
                MaxVal = table.Column<float>(type: "real", nullable: true),
                MetadataId = table.Column<Guid>(type: "uuid", nullable: false),
                DeviceId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SensorMetadata", x => x.Id);
                table.ForeignKey(
                    name: "FK_SensorMetadata_Metadatas_MetadataId",
                    column: x => x.MetadataId,
                    principalTable: "Metadatas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SensorMetadata_Sensors_DeviceId",
                    column: x => x.DeviceId,
                    principalTable: "Sensors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ActuatorMetadatas",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                NumericValue = table.Column<float>(type: "real", nullable: true),
                StringValue = table.Column<string>(type: "text", nullable: false),
                TypeValue = table.Column<int>(type: "integer", nullable: true),
                Unit = table.Column<string>(type: "text", nullable: false),
                MinVal = table.Column<float>(type: "real", nullable: true),
                MaxVal = table.Column<float>(type: "real", nullable: true),
                MetadataId = table.Column<Guid>(type: "uuid", nullable: false),
                DeviceId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ActuatorMetadatas", x => x.Id);
                table.ForeignKey(
                    name: "FK_ActuatorMetadatas_Actuators_DeviceId",
                    column: x => x.DeviceId,
                    principalTable: "Actuators",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ActuatorMetadatas_Metadatas_MetadataId",
                    column: x => x.MetadataId,
                    principalTable: "Metadatas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ActuatorMetadatas_DeviceId",
            table: "ActuatorMetadatas",
            column: "DeviceId");

        migrationBuilder.CreateIndex(
            name: "IX_ActuatorMetadatas_MetadataId",
            table: "ActuatorMetadatas",
            column: "MetadataId");

        migrationBuilder.CreateIndex(
            name: "IX_Actuators_SensorId",
            table: "Actuators",
            column: "SensorId");

        migrationBuilder.CreateIndex(
            name: "IX_Actuators_SourceDeviceId",
            table: "Actuators",
            column: "SourceDeviceId");

        migrationBuilder.CreateIndex(
            name: "IX_Dashboards_UserId",
            table: "Dashboards",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_DeviceMetadata_DeviceId",
            table: "DeviceMetadata",
            column: "DeviceId");

        migrationBuilder.CreateIndex(
            name: "IX_DeviceMetadata_MetadataId",
            table: "DeviceMetadata",
            column: "MetadataId");

        migrationBuilder.CreateIndex(
            name: "IX_DeviceVersions_SoftwareId",
            table: "DeviceVersions",
            column: "SoftwareId");

        migrationBuilder.CreateIndex(
            name: "IX_Preference_SubscriberId",
            table: "Preference",
            column: "SubscriberId");

        migrationBuilder.CreateIndex(
            name: "IX_RuleConnector_ContinuingRuleId",
            table: "RuleConnector",
            column: "ContinuingRuleId");

        migrationBuilder.CreateIndex(
            name: "IX_RuleParameter_TargetRuleId",
            table: "RuleParameter",
            column: "TargetRuleId");

        migrationBuilder.CreateIndex(
            name: "IX_SensorMetadata_DeviceId",
            table: "SensorMetadata",
            column: "DeviceId");

        migrationBuilder.CreateIndex(
            name: "IX_SensorMetadata_MetadataId",
            table: "SensorMetadata",
            column: "MetadataId");

        migrationBuilder.CreateIndex(
            name: "IX_Sensors_SourceDeviceId",
            table: "Sensors",
            column: "SourceDeviceId");

        migrationBuilder.CreateIndex(
            name: "IX_SoftwareVersions_SoftwareId",
            table: "SoftwareVersions",
            column: "SoftwareId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ActuatorMetadatas");

        migrationBuilder.DropTable(
            name: "Dashboards");

        migrationBuilder.DropTable(
            name: "DashboardsDevice");

        migrationBuilder.DropTable(
            name: "DeviceMetadata");

        migrationBuilder.DropTable(
            name: "DeviceVersions");

        migrationBuilder.DropTable(
            name: "Events");

        migrationBuilder.DropTable(
            name: "Preference");

        migrationBuilder.DropTable(
            name: "RuleConnector");

        migrationBuilder.DropTable(
            name: "RuleParameter");

        migrationBuilder.DropTable(
            name: "SensorMetadata");

        migrationBuilder.DropTable(
            name: "SoftwareVersions");

        migrationBuilder.DropTable(
            name: "Actuators");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropTable(
            name: "Rules");

        migrationBuilder.DropTable(
            name: "Metadatas");

        migrationBuilder.DropTable(
            name: "Softwares");

        migrationBuilder.DropTable(
            name: "Sensors");

        migrationBuilder.DropTable(
            name: "Devices");
    }
}
