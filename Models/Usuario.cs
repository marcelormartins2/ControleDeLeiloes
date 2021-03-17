using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace ControleDeLeiloes.Models
{
    public class Usuario : IdentityUser
    {

        public string Nome { get; set; }
        public string Celular { get; set; }
        [Display(Name = "Endereço")]
        public string Endereco { get; set; }

        public static implicit operator int(Usuario v)
        {
            throw new NotImplementedException();
        }
    }
}
