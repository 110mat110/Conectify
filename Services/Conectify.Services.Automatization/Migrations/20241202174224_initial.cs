using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Services.Automatization.Migrations;

/// <inheritdoc />
public partial class initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
            name: "InputConnectors",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                Index = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InputConnectors", x => x.Id);
                table.ForeignKey(
                    name: "FK_InputConnectors_Rules_RuleId",
                    column: x => x.RuleId,
                    principalTable: "Rules",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OutputConnectors",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                Index = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OutputConnectors", x => x.Id);
                table.ForeignKey(
                    name: "FK_OutputConnectors_Rules_RuleId",
                    column: x => x.RuleId,
                    principalTable: "Rules",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RuleConnector",
            columns: table => new
            {
                SourceRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                TargetRuleId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RuleConnector", x => new { x.SourceRuleId, x.TargetRuleId });
                table.ForeignKey(
                    name: "FK_RuleConnector_InputConnectors_TargetRuleId",
                    column: x => x.TargetRuleId,
                    principalTable: "InputConnectors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RuleConnector_OutputConnectors_SourceRuleId",
                    column: x => x.SourceRuleId,
                    principalTable: "OutputConnectors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_InputConnectors_RuleId",
            table: "InputConnectors",
            column: "RuleId");

        migrationBuilder.CreateIndex(
            name: "IX_OutputConnectors_RuleId",
            table: "OutputConnectors",
            column: "RuleId");

        migrationBuilder.CreateIndex(
            name: "IX_RuleConnector_TargetRuleId",
            table: "RuleConnector",
            column: "TargetRuleId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RuleConnector");

        migrationBuilder.DropTable(
            name: "InputConnectors");

        migrationBuilder.DropTable(
            name: "OutputConnectors");

        migrationBuilder.DropTable(
            name: "Rules");
    }
}
