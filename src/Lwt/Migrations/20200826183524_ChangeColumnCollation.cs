using Microsoft.EntityFrameworkCore.Migrations;

namespace Lwt.Migrations
{
    public partial class ChangeColumnCollation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `terms` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_bin;");
            migrationBuilder.Sql("ALTER TABLE `texts` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_bin;");
            migrationBuilder.Sql("ALTER TABLE `text_terms` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_bin;");
            migrationBuilder.Sql("ALTER TABLE `user_settings` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_bin;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `terms` CONVERT TO CHARACTER SET utf8 COLLATE utf8_general_ci;");
            migrationBuilder.Sql("ALTER TABLE `texts` CONVERT TO CHARACTER SET utf8 COLLATE utf8_general_ci;");
            migrationBuilder.Sql("ALTER TABLE `text_terms` CONVERT TO CHARACTER SET utf8 COLLATE utf8_general_ci;");
            migrationBuilder.Sql("ALTER TABLE `user_settings` CONVERT TO CHARACTER SET utf8 COLLATE utf8_general_ci;");
        }
    }
}
