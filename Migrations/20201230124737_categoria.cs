using Microsoft.EntityFrameworkCore.Migrations;

namespace ControleDeLeiloes.Migrations
{
    public partial class categoria : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Anuncio",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subcategoria",
                table: "Anuncio",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Anuncio");

            migrationBuilder.DropColumn(
                name: "Subcategoria",
                table: "Anuncio");
        }
    }
}
