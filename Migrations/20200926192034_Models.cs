using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ControleDeLeiloes.Migrations
{
    public partial class Models : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leiloeiro",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(nullable: true),
                    Telefone = table.Column<string>(nullable: true),
                    Site = table.Column<string>(nullable: true),
                    TaxaAvaliacaoPadrao = table.Column<double>(nullable: false),
                    TaxaVendaPadrao = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leiloeiro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Produto",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(nullable: true),
                    Data = table.Column<DateTime>(nullable: false),
                    VlAnunciado = table.Column<double>(nullable: true),
                    VlNegociado = table.Column<double>(nullable: true),
                    VlCompra = table.Column<double>(nullable: true),
                    VlVenda = table.Column<double>(nullable: true),
                    Bairro = table.Column<string>(nullable: true),
                    Endereco = table.Column<string>(nullable: true),
                    Localizacao = table.Column<string>(nullable: true),
                    Anuncio = table.Column<string>(nullable: true),
                    Telefone = table.Column<string>(nullable: true),
                    Vendedor = table.Column<string>(nullable: true),
                    UsuarioId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produto_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Leilao",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(nullable: true),
                    Data = table.Column<DateTime>(nullable: false),
                    TaxaAvaliacao = table.Column<double>(nullable: false),
                    TaxaVenda = table.Column<double>(nullable: false),
                    LeiloeiroId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leilao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leilao_Leiloeiro_LeiloeiroId",
                        column: x => x.LeiloeiroId,
                        principalTable: "Leiloeiro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lote",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Numero = table.Column<string>(nullable: true),
                    VlAvalicao = table.Column<double>(nullable: false),
                    VlCondicional = table.Column<double>(nullable: true),
                    VlPago = table.Column<double>(nullable: true),
                    VlLance = table.Column<double>(nullable: true),
                    ProdutoId1 = table.Column<string>(nullable: true),
                    ProdutoId = table.Column<int>(nullable: false),
                    LeilaoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lote_Leilao_LeilaoId",
                        column: x => x.LeilaoId,
                        principalTable: "Leilao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lote_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lote_AspNetUsers_ProdutoId1",
                        column: x => x.ProdutoId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leilao_LeiloeiroId",
                table: "Leilao",
                column: "LeiloeiroId");

            migrationBuilder.CreateIndex(
                name: "IX_Lote_LeilaoId",
                table: "Lote",
                column: "LeilaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lote_ProdutoId",
                table: "Lote",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lote_ProdutoId1",
                table: "Lote",
                column: "ProdutoId1");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_UsuarioId",
                table: "Produto",
                column: "UsuarioId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lote");

            migrationBuilder.DropTable(
                name: "Leilao");

            migrationBuilder.DropTable(
                name: "Produto");

            migrationBuilder.DropTable(
                name: "Leiloeiro");
        }
    }
}
