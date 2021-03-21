using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControleDeLeiloes.Models
{
    public class Leilao
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public string Descricao { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Data { get; set; }
        public double TaxaAvaliacao { get; set; }
        public double TaxaVenda { get; set; }

        public Leiloeiro Leiloeiro { get; set; }
        public int LeiloeiroId { get; set; }
        public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
    }
}
