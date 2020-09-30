using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ControleDeLeiloes.Migrations
{
    public partial class AnuncioEvendedorProibido : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Anuncio",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdAnuncio = table.Column<int>(nullable: false),
                    Link = table.Column<string>(nullable: true),
                    DtPublicacao = table.Column<DateTime>(nullable: false),
                    Img1 = table.Column<string>(nullable: true),
                    Img2 = table.Column<string>(nullable: true),
                    Img3 = table.Column<string>(nullable: true),
                    Descricao = table.Column<string>(nullable: true),
                    VlAnunciado = table.Column<double>(nullable: true),
                    Bairro = table.Column<string>(nullable: true),
                    Telefone = table.Column<string>(nullable: true),
                    Vendedor = table.Column<string>(nullable: true),
                    IdVendedor = table.Column<int>(nullable: false),
                    DtVendedorDesde = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anuncio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendedorProibido",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdVendedor = table.Column<int>(nullable: false),
                    Nome = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendedorProibido", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendedorProibido_IdVendedor",
                table: "VendedorProibido",
                column: "IdVendedor");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anuncio");

            migrationBuilder.DropTable(
                name: "VendedorProibido");
        }
    }
}
