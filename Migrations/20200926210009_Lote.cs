using Microsoft.EntityFrameworkCore.Migrations;

namespace ControleDeLeiloes.Migrations
{
    public partial class Lote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lote_AspNetUsers_ProdutoId1",
                table: "Lote");

            migrationBuilder.DropIndex(
                name: "IX_Lote_ProdutoId1",
                table: "Lote");

            migrationBuilder.DropColumn(
                name: "ProdutoId1",
                table: "Lote");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProdutoId1",
                table: "Lote",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lote_ProdutoId1",
                table: "Lote",
                column: "ProdutoId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Lote_AspNetUsers_ProdutoId1",
                table: "Lote",
                column: "ProdutoId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
