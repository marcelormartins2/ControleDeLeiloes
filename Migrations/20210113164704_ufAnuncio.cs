using Microsoft.EntityFrameworkCore.Migrations;

namespace ControleDeLeiloes.Migrations
{
    public partial class ufAnuncio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UF",
                table: "Anuncio",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UF",
                table: "Anuncio");
        }
    }
}
