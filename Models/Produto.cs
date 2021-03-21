using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ControleDeLeiloes.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}")]
        public DateTime DataCadastro { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}")]
        public DateTime DataAnuncio { get; set; }

        [DisplayName(displayName: "Valor Anunciado")]
        [DisplayFormat(DataFormatString = "{0:0.00}")]
        public double? VlAnunciado { get; set; }

        [DisplayName(displayName: "Valor Negociado")]
        [DisplayFormat(DataFormatString = "{0:0.00}")]
        public double? VlNegociado { get; set; }

        [DisplayName(displayName: "Valor Compra")]
        [DisplayFormat(DataFormatString = "{0:0.00}")]
        public double? VlCompra { get; set; }

        [DisplayName(displayName: "Valor de Venda")]
        [DisplayFormat(DataFormatString = "{0:0.00}")]
        public double? VlVenda { get; set; }
        public string Bairro { get; set; }
        public string Endereco { get; set; }
        public string Localizacao{ get; set; }
        public string Anuncio { get; set; }
        public string Telefone { get; set; }
        public string Vendedor { get; set; }
        public Usuario Usuario { get; set; }
        [Required()]
        public string UsuarioId { get; set; }
        public ICollection<Foto> Fotos { get; set; } = new List<Foto>();
        public IList<LoteProduto> LoteProdutos { get; set; }
    }
}
