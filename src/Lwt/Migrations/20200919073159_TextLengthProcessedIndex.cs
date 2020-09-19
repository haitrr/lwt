namespace Lwt.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class TextLengthProcessedIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessedTermCount",
                table: "texts");

            migrationBuilder.AddColumn<int>(
                name: "Length",
                table: "texts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProcessedIndex",
                table: "texts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Length",
                table: "texts");

            migrationBuilder.DropColumn(
                name: "ProcessedIndex",
                table: "texts");

            migrationBuilder.AddColumn<int>(
                name: "ProcessedTermCount",
                table: "texts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
