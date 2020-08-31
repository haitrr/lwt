namespace Lwt.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddTermCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TermCount",
                table: "texts",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TermCount",
                table: "texts");
        }
    }
}
