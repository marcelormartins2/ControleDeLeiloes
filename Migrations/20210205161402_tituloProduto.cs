using Microsoft.EntityFrameworkCore.Migrations;

namespace ControleDeLeiloes.Migrations
{
    public partial class tituloProduto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Produto",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "Produto");
        }
    }
}
