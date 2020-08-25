using Microsoft.EntityFrameworkCore.Migrations;

namespace Lwt.Migrations
{
    public partial class AddCombineUniqueIndexForTerm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LanguageCode",
                table: "terms",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "terms",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_terms_Content_LanguageCode_UserId",
                table: "terms",
                columns: new[] { "Content", "LanguageCode", "UserId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_terms_Content_LanguageCode_UserId",
                table: "terms");

            migrationBuilder.AlterColumn<string>(
                name: "LanguageCode",
                table: "terms",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "terms",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
