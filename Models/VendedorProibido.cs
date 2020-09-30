using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleDeLeiloes.Models
{
    public class VendedorProibido
    {
        public int Id { get; set; }
        public int IdVendedor { get; set; }
        public string Nome { get; set; }
    }
}
