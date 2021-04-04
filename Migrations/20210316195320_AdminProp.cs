using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VLO_BOARDS.Migrations
{
    public partial class AdminProp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ThreadId",
                table: "Property",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ThreadId1",
                table: "Property",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdminProperty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminProperty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminProperty_Property_Id",
                        column: x => x.Id,
                        principalTable: "Property",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EditOrDeleteProperty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditOrDeleteProperty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditOrDeleteProperty_Property_Id",
                        column: x => x.Id,
                        principalTable: "Property",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReadProperty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadProperty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReadProperty_Property_Id",
                        column: x => x.Id,
                        principalTable: "Property",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViewProperty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewProperty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViewProperty_Property_Id",
                        column: x => x.Id,
                        principalTable: "Property",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WriteProperty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WriteProperty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WriteProperty_Property_Id",
                        column: x => x.Id,
                        principalTable: "Property",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Property_ThreadId",
                table: "Property",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_ThreadId1",
                table: "Property",
                column: "ThreadId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Property_Threads_ThreadId",
                table: "Property",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Property_Threads_ThreadId1",
                table: "Property",
                column: "ThreadId1",
                principalTable: "Threads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Property_Threads_ThreadId",
                table: "Property");

            migrationBuilder.DropForeignKey(
                name: "FK_Property_Threads_ThreadId1",
                table: "Property");

            migrationBuilder.DropTable(
                name: "AdminProperty");

            migrationBuilder.DropTable(
                name: "EditOrDeleteProperty");

            migrationBuilder.DropTable(
                name: "ReadProperty");

            migrationBuilder.DropTable(
                name: "ViewProperty");

            migrationBuilder.DropTable(
                name: "WriteProperty");

            migrationBuilder.DropIndex(
                name: "IX_Property_ThreadId",
                table: "Property");

            migrationBuilder.DropIndex(
                name: "IX_Property_ThreadId1",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "ThreadId",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "ThreadId1",
                table: "Property");
        }
    }
}
