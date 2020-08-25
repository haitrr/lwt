using Microsoft.EntityFrameworkCore.Migrations;

namespace Lwt.Migrations
{
    public partial class AddUserSettingFKUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_user_settings_UserId",
                table: "user_settings",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_user_settings_users_UserId",
                table: "user_settings",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_settings_users_UserId",
                table: "user_settings");

            migrationBuilder.DropIndex(
                name: "IX_user_settings_UserId",
                table: "user_settings");
        }
    }
}
