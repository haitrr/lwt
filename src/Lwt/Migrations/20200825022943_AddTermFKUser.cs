using Microsoft.EntityFrameworkCore.Migrations;

namespace Lwt.Migrations
{
    public partial class AddTermFKUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "CreatorId", newName: "UserId", table: "terms");

            migrationBuilder.CreateIndex(name: "IX_terms_UserId", table: "terms", column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_terms_users_UserId",
                table: "terms",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_terms_users_UserId", table: "terms");

            migrationBuilder.DropIndex(name: "IX_terms_UserId", table: "terms");

            migrationBuilder.RenameColumn(name: "UserId", newName: "CreatorId", table: "terms");
        }
    }
}