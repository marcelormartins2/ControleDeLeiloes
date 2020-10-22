using Microsoft.EntityFrameworkCore.Migrations;

namespace ControleDeLeiloes.Migrations
{
    public partial class IdAnunciostring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdAnuncio",
                table: "Anuncio",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IdAnuncio",
                table: "Anuncio",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
