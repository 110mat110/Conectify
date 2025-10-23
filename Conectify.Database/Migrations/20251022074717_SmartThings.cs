using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Conectify.Database.Migrations
{
    /// <inheritdoc />
    public partial class SmartThings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RuleConnector");

            migrationBuilder.DropTable(
                name: "RuleParameter");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.CreateTable(
                name: "SmartThings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Capability = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartThings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmartThingsTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartThingsTokens", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmartThings");

            migrationBuilder.DropTable(
                name: "SmartThingsTokens");

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
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
                name: "RuleConnector",
                columns: table => new
                {
                    PreviousRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContinuingRuleId = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_RuleConnector_ContinuingRuleId",
                table: "RuleConnector",
                column: "ContinuingRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleParameter_TargetRuleId",
                table: "RuleParameter",
                column: "TargetRuleId");
        }
    }
}
