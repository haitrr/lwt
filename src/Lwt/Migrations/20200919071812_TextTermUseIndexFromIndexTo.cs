namespace Lwt.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class TextTermUseIndexFromIndexTo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Index", table: "text_terms");

            migrationBuilder.AddColumn<int>(name: "IndexFrom", table: "text_terms", nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<int>(name: "IndexTo", table: "text_terms", nullable: false, defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IndexFrom", table: "text_terms");

            migrationBuilder.DropColumn(name: "IndexTo", table: "text_terms");

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "text_terms",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}