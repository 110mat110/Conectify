using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations;

public partial class typoInDevice : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "SubscibeToAll",
            table: "Devices",
            newName: "SubscribeToAll");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "SubscribeToAll",
            table: "Devices",
            newName: "SubscibeToAll");
    }
}
