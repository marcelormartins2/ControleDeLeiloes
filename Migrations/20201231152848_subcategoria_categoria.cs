using Microsoft.EntityFrameworkCore.Migrations;

namespace ControleDeLeiloes.Migrations
{
    public partial class subcategoria_categoria : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anuncio_CategoriaAnuncio_CategoriaAnuncioId",
                table: "Anuncio");

            migrationBuilder.DropIndex(
                name: "IX_Anuncio_CategoriaAnuncioId",
                table: "Anuncio");

            migrationBuilder.DropColumn(
                name: "CategoriaAnuncioId",
                table: "Anuncio");

            migrationBuilder.AddColumn<int>(
                name: "CategoriaAnuncioId",
                table: "SubcategoriaAnuncio",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SubcategoriaAnuncio_CategoriaAnuncioId",
                table: "SubcategoriaAnuncio",
                column: "CategoriaAnuncioId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubcategoriaAnuncio_CategoriaAnuncio_CategoriaAnuncioId",
                table: "SubcategoriaAnuncio",
                column: "CategoriaAnuncioId",
                principalTable: "CategoriaAnuncio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubcategoriaAnuncio_CategoriaAnuncio_CategoriaAnuncioId",
                table: "SubcategoriaAnuncio");

            migrationBuilder.DropIndex(
                name: "IX_SubcategoriaAnuncio_CategoriaAnuncioId",
                table: "SubcategoriaAnuncio");

            migrationBuilder.DropColumn(
                name: "CategoriaAnuncioId",
                table: "SubcategoriaAnuncio");

            migrationBuilder.AddColumn<int>(
                name: "CategoriaAnuncioId",
                table: "Anuncio",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Anuncio_CategoriaAnuncioId",
                table: "Anuncio",
                column: "CategoriaAnuncioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Anuncio_CategoriaAnuncio_CategoriaAnuncioId",
                table: "Anuncio",
                column: "CategoriaAnuncioId",
                principalTable: "CategoriaAnuncio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
