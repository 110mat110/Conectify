using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Services.Automatization.Migrations
{
    /// <inheritdoc />
    public partial class AddFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FunctionId",
                table: "Rules",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Functions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ParentFunctionId = table.Column<Guid>(type: "uuid", nullable: true),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Functions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Functions_Functions_ParentFunctionId",
                        column: x => x.ParentFunctionId,
                        principalTable: "Functions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rules_FunctionId",
                table: "Rules",
                column: "FunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Functions_ParentFunctionId",
                table: "Functions",
                column: "ParentFunctionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rules_Functions_FunctionId",
                table: "Rules",
                column: "FunctionId",
                principalTable: "Functions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rules_Functions_FunctionId",
                table: "Rules");

            migrationBuilder.DropTable(
                name: "Functions");

            migrationBuilder.DropIndex(
                name: "IX_Rules_FunctionId",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "FunctionId",
                table: "Rules");
        }
    }
}
