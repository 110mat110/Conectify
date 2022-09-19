using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations
{
    public partial class subsrcibers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Preference_SubscriberId",
                table: "Preference",
                column: "SubscriberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Preference_Devices_SubscriberId",
                table: "Preference",
                column: "SubscriberId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Preference_Devices_SubscriberId",
                table: "Preference");

            migrationBuilder.DropIndex(
                name: "IX_Preference_SubscriberId",
                table: "Preference");
        }
    }
}
