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
        public string IdAnuncio { get; set; }
        public string Titulo { get; set; }
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
        public string IdVendedor { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime DtVendedorDesde { get; set; }
        public bool NotView { get; set; }

        //Caso seja preciso clonar a classe - usado para evitar a associação por ponteiro na memória
        //public Anuncio Clone ()
        //{
        //    return (Anuncio)this.MemberwiseClone();
        //}

    }
}
