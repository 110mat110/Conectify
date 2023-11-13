using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations;

public partial class RuleParameters : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
            name: "IX_RuleParameter_TargetRuleId",
            table: "RuleParameter",
            column: "TargetRuleId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RuleParameter");
    }
}
