using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LWT.Migrations
{
    public partial class UpdateLanguageModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Texts_Languages_LanguageID",
                table: "Texts");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Texts",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LanguageID",
                table: "Texts",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Texts",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Languages",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentenceSplitPattern",
                table: "Languages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WordSplitPattern",
                table: "Languages",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Texts_Languages_LanguageID",
                table: "Texts",
                column: "LanguageID",
                principalTable: "Languages",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Texts_Languages_LanguageID",
                table: "Texts");

            migrationBuilder.DropColumn(
                name: "SentenceSplitPattern",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "WordSplitPattern",
                table: "Languages");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Texts",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "LanguageID",
                table: "Texts",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Texts",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Languages",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_Texts_Languages_LanguageID",
                table: "Texts",
                column: "LanguageID",
                principalTable: "Languages",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
