using Microsoft.EntityFrameworkCore.Migrations;

namespace Lwt.Migrations
{
    public partial class AddIndexForGetTermCountInTextQuery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_text_terms_TextId",
                table: "text_terms");

            migrationBuilder.CreateIndex(
                name: "IX_text_terms_TextId_TermId",
                table: "text_terms",
                columns: new[] { "TextId", "TermId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_text_terms_TextId_TermId",
                table: "text_terms");

            migrationBuilder.CreateIndex(
                name: "IX_text_terms_TextId",
                table: "text_terms",
                column: "TextId");
        }
    }
}
