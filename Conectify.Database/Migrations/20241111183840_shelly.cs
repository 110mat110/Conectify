using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations;

/// <inheritdoc />
public partial class shelly : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Shellys",
            columns: table => new
            {
                ShellyId = table.Column<string>(type: "text", nullable: false),
                Json = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Shellys", x => x.ShellyId);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Shellys");
    }
}
