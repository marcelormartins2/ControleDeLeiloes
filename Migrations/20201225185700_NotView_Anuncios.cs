using Microsoft.EntityFrameworkCore.Migrations;

namespace ControleDeLeiloes.Migrations
{
    public partial class NotView_Anuncios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotView",
                table: "Anuncio",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotView",
                table: "Anuncio");
        }
    }
}
