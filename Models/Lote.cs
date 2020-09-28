using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleDeLeiloes.Models
{

    public class Lote
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public double VlAvalicao { get; set; }
        public double? VlCondicional { get; set; }
        public double? VlPago { get; set; }
        public double? VlLance { get; set; }

        public Produto Produto { get; set; }
        public int ProdutoId { get; set; }
        public Leilao Leilao { get; set; }
        public int LeilaoId { get; set; }
    }
}
