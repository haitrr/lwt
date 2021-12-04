using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lwt.Migrations
{
    public partial class AddEntityInfoInLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "logs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "logs",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "logs");
        }
    }
}
