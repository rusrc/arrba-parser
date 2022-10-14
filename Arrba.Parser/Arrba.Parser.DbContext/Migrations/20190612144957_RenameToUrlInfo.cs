using Microsoft.EntityFrameworkCore.Migrations;

namespace Arrba.Parser.DbContext.Migrations
{
    public partial class RenameToUrlInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urls_UrlInfos_LinksRequestId",
                table: "Urls");

            migrationBuilder.RenameColumn(
                name: "LinksRequestId",
                table: "Urls",
                newName: "UrlInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Urls_LinksRequestId",
                table: "Urls",
                newName: "IX_Urls_UrlInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Urls_UrlInfos_UrlInfoId",
                table: "Urls",
                column: "UrlInfoId",
                principalTable: "UrlInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urls_UrlInfos_UrlInfoId",
                table: "Urls");

            migrationBuilder.RenameColumn(
                name: "UrlInfoId",
                table: "Urls",
                newName: "LinksRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Urls_UrlInfoId",
                table: "Urls",
                newName: "IX_Urls_LinksRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Urls_UrlInfos_LinksRequestId",
                table: "Urls",
                column: "LinksRequestId",
                principalTable: "UrlInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
