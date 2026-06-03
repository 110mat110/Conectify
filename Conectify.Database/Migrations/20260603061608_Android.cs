using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations
{
    /// <inheritdoc />
    public partial class Android : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AndroidWidgetItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserMail = table.Column<string>(type: "text", nullable: false),
                    WidgetType = table.Column<string>(type: "text", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AndroidWidgetItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AndroidWidgetItems_UserMail_WidgetType",
                table: "AndroidWidgetItems",
                columns: new[] { "UserMail", "WidgetType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AndroidWidgetItems");
        }
    }
}
