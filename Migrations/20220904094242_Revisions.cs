using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VLO_BOARDS.Migrations
{
    public partial class Revisions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Revision_Articles_ArticleId",
                table: "Revision");

            migrationBuilder.AlterColumn<string>(
                name: "ArticleId",
                table: "Revision",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Revision_Articles_ArticleId",
                table: "Revision",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Revision_Articles_ArticleId",
                table: "Revision");

            migrationBuilder.AlterColumn<string>(
                name: "ArticleId",
                table: "Revision",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Revision_Articles_ArticleId",
                table: "Revision",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "ArticleId");
        }
    }
}
