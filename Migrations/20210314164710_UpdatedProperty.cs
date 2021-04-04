using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VLO_BOARDS.Migrations
{
    public partial class UpdatedProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Property_Property_PropertyId",
                table: "Property");

            migrationBuilder.DropTable(
                name: "PropertiesSets");

            migrationBuilder.DropIndex(
                name: "IX_Property_PropertyId",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "propertiesId",
                table: "Property");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PropertyId",
                table: "Property",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "propertiesId",
                table: "Property",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "PropertiesSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertiesSets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Property_PropertyId",
                table: "Property",
                column: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Property_Property_PropertyId",
                table: "Property",
                column: "PropertyId",
                principalTable: "Property",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
