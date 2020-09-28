using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleDeLeiloes.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ControleDeLeiloes.Areas.Identity.Pages.Account
{
    public class RegistroConfirmado : PageModel
    {
        public RegistroConfirmado()
        {
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }

}
