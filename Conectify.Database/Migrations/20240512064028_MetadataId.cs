using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectify.Database.Migrations
{
    public partial class MetadataId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorMetadata",
                table: "SensorMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceMetadata",
                table: "DeviceMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActuatorMetadatas",
                table: "ActuatorMetadatas");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SensorMetadata",
                type: "uuid",
                nullable: false,
               defaultValueSql: "gen_random_uuid ()");

            migrationBuilder.AddColumn<bool>(
                name: "Exclusive",
                table: "Metadatas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "DeviceMetadata",
                type: "uuid",
                nullable: false,
       defaultValueSql: "gen_random_uuid ()");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ActuatorMetadatas",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid ()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorMetadata",
                table: "SensorMetadata",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceMetadata",
                table: "DeviceMetadata",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActuatorMetadatas",
                table: "ActuatorMetadatas",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SensorMetadata_DeviceId",
                table: "SensorMetadata",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMetadata_DeviceId",
                table: "DeviceMetadata",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_ActuatorMetadatas_DeviceId",
                table: "ActuatorMetadatas",
                column: "DeviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorMetadata",
                table: "SensorMetadata");

            migrationBuilder.DropIndex(
                name: "IX_SensorMetadata_DeviceId",
                table: "SensorMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceMetadata",
                table: "DeviceMetadata");

            migrationBuilder.DropIndex(
                name: "IX_DeviceMetadata_DeviceId",
                table: "DeviceMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActuatorMetadatas",
                table: "ActuatorMetadatas");

            migrationBuilder.DropIndex(
                name: "IX_ActuatorMetadatas_DeviceId",
                table: "ActuatorMetadatas");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SensorMetadata");

            migrationBuilder.DropColumn(
                name: "Exclusive",
                table: "Metadatas");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DeviceMetadata");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ActuatorMetadatas");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorMetadata",
                table: "SensorMetadata",
                columns: new[] { "DeviceId", "MetadataId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceMetadata",
                table: "DeviceMetadata",
                columns: new[] { "DeviceId", "MetadataId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActuatorMetadatas",
                table: "ActuatorMetadatas",
                columns: new[] { "DeviceId", "MetadataId" });
        }
    }
}
