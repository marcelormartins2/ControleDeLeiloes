using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ControleDeLeiloes.Models
{
    public class Leiloeiro
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Site { get; set; }
        public double TaxaAvaliacaoPadrao { get; set; }
        public double TaxaVendaPadrao { get; set; }
    }
}
