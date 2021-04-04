using Microsoft.EntityFrameworkCore.Migrations;

namespace VLO_BOARDS.Migrations
{
    public partial class Initialize2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Property_Boards_BoardName2",
                table: "Property");

            migrationBuilder.DropIndex(
                name: "IX_Property_BoardName2",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "BoardName2",
                table: "Property");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BoardName2",
                table: "Property",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Property_BoardName2",
                table: "Property",
                column: "BoardName2");

            migrationBuilder.AddForeignKey(
                name: "FK_Property_Boards_BoardName2",
                table: "Property",
                column: "BoardName2",
                principalTable: "Boards",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
