using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleDeLeiloes.Models
{
    public class Progresso
    {
        public int EtapaPagina { get; set; }
        public int EtapaAnuncio { get; set; }
        public int EtapaDados { get; set; }
        public int QuantidadePaginas { get; set; }
        public int QuantidadeAnuncios { get; set; }
        public string Estagio { get; set; }
        public bool Gravado { get; set; }
        public String MensagemErro { get; set; }
        public int RegistrosBD { get; set; }
        public int RegistrosBDAnalisados { get; set; }
        public int RegistrosBDApagados { get; set; }

    }
}
