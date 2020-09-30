using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ControleDeLeiloes.Models
{
    public class Anuncio
    {
        public int Id { get; set; }
        public int IdAnuncio { get; set; }
        public string Link { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}")]
        public DateTime DtPublicacao { get; set; }
        public string Img1 { get; set; }
        public string Img2 { get; set; }
        public string Img3 { get; set; }
        public string Descricao { get; set; }
        [DisplayName(displayName: "Valor Anunciado")]
        public double? VlAnunciado { get; set; }
        [DisplayName(displayName: "Valor Negociado")]
        public string Bairro { get; set; }
        public string Telefone { get; set; }
        public string Vendedor { get; set; }
        public int CodVendedor { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime DtVendedorDesde { get; set; }

    }
}
