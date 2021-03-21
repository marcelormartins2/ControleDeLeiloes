using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleDeLeiloes.Models
{
    public class LoteProduto
    {
        public int LoteId { get; set; }
        public Lote Lote { get; set; }
        public int ProdtoId { get; set; }
        public Produto Produto { get; set; }
    }
}
