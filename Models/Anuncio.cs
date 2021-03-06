﻿using System;
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
        public string Descricao { get; set; }
        [DisplayName(displayName: "Valor Anunciado")]
        public double? VlAnunciado { get; set; }
        [DisplayName(displayName: "Valor Negociado")]
        public string UF { get; set; }
        public string Bairro { get; set; }
        public string Telefone { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime DtVendedorDesde { get; set; }
        public bool NotView { get; set; }
        public bool OlxPay { get; set; }
        public bool OlxDelivery { get; set; }
        public string Vendedor { get; set; }
        public string OlxIdVendedor { get; set; }
        public SubcategoriaAnuncio SubcategoriaAnuncio { get; set; }
        public int SubcategoriaAnuncioId { get; set; }
        public ICollection<Foto> Fotos { get; set; } = new List<Foto>();

        //Caso seja preciso clonar a classe - usado para evitar a associação por ponteiro na memória
        //public Anuncio Clone ()
        //{
        //    return (Anuncio)this.MemberwiseClone();
        //}

    }
}
