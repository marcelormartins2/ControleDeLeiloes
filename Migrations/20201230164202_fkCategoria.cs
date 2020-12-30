using Microsoft.EntityFrameworkCore.Migrations;

namespace ControleDeLeiloes.Migrations
{
    public partial class fkCategoria : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Anuncio");

            migrationBuilder.DropColumn(
                name: "Subcategoria",
                table: "Anuncio");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "SubcategoriaAnuncio",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "CategoriaAnuncio",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CategoriaAnuncioId",
                table: "Anuncio",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubcategoriaAnuncioId",
                table: "Anuncio",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Anuncio_CategoriaAnuncioId",
                table: "Anuncio",
                column: "CategoriaAnuncioId");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncio_SubcategoriaAnuncioId",
                table: "Anuncio",
                column: "SubcategoriaAnuncioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Anuncio_CategoriaAnuncio_CategoriaAnuncioId",
                table: "Anuncio",
                column: "CategoriaAnuncioId",
                principalTable: "CategoriaAnuncio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Anuncio_SubcategoriaAnuncio_SubcategoriaAnuncioId",
                table: "Anuncio",
                column: "SubcategoriaAnuncioId",
                principalTable: "SubcategoriaAnuncio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anuncio_CategoriaAnuncio_CategoriaAnuncioId",
                table: "Anuncio");

            migrationBuilder.DropForeignKey(
                name: "FK_Anuncio_SubcategoriaAnuncio_SubcategoriaAnuncioId",
                table: "Anuncio");

            migrationBuilder.DropIndex(
                name: "IX_Anuncio_CategoriaAnuncioId",
                table: "Anuncio");

            migrationBuilder.DropIndex(
                name: "IX_Anuncio_SubcategoriaAnuncioId",
                table: "Anuncio");

            migrationBuilder.DropColumn(
                name: "CategoriaAnuncioId",
                table: "Anuncio");

            migrationBuilder.DropColumn(
                name: "SubcategoriaAnuncioId",
                table: "Anuncio");

            migrationBuilder.AlterColumn<int>(
                name: "Nome",
                table: "SubcategoriaAnuncio",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Nome",
                table: "CategoriaAnuncio",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Anuncio",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subcategoria",
                table: "Anuncio",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
