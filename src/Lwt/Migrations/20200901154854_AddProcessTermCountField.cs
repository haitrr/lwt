using Microsoft.EntityFrameworkCore.Migrations;

namespace Lwt.Migrations
{
    public partial class AddProcessTermCountField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcessedTermCount",
                table: "texts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessedTermCount",
                table: "texts");
        }
    }
}
