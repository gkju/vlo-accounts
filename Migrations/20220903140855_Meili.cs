using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VLO_BOARDS.Migrations
{
    public partial class Meili : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentText",
                table: "Articles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentText",
                table: "Articles");
        }
    }
}
