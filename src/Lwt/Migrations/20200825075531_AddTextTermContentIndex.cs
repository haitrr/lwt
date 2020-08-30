using Microsoft.EntityFrameworkCore.Migrations;

namespace Lwt.Migrations
{
    public partial class AddTextTermContentIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.Sql(
               // "CREATE INDEX index_text_term_content ON text_terms ((cast(Lower(Content) as CHAR(256))));");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(name: "index_text_term_content", table: "text_terms");
        }
    }
}