using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations;

public partial class CoordsForRules : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "X",
            table: "Rules",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "Y",
            table: "Rules",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "X",
            table: "Rules");

        migrationBuilder.DropColumn(
            name: "Y",
            table: "Rules");
    }
}
