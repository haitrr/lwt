using Microsoft.EntityFrameworkCore.Migrations;

namespace Lwt.Migrations
{
    public partial class AddTextFKUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "CreatorId", newName: "UserId", table: "texts");

            migrationBuilder.CreateIndex(name: "IX_texts_UserId", table: "texts", column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_texts_users_UserId",
                table: "texts",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_texts_users_UserId", table: "texts");

            migrationBuilder.DropIndex(name: "IX_texts_UserId", table: "texts");

            migrationBuilder.RenameColumn(name: "UserId", newName: "CreatorId", table: "texts");
        }
    }
}