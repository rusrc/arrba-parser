using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Arrba.Parser.DbContext.Migrations
{
    public partial class AddPayloadEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PayloadId",
                table: "Urls",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Payloads",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CategoryName = table.Column<string>(nullable: true),
                    TypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payloads", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Urls_PayloadId",
                table: "Urls",
                column: "PayloadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Urls_Payloads_PayloadId",
                table: "Urls",
                column: "PayloadId",
                principalTable: "Payloads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urls_Payloads_PayloadId",
                table: "Urls");

            migrationBuilder.DropTable(
                name: "Payloads");

            migrationBuilder.DropIndex(
                name: "IX_Urls_PayloadId",
                table: "Urls");

            migrationBuilder.DropColumn(
                name: "PayloadId",
                table: "Urls");
        }
    }
}
