using ControleDeLeiloes.Data;
using ControleDeLeiloes.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ControleDeLeiloes.Controllers
{
    public class ProdutosController : Controller
    {

        private readonly ControleDeLeiloesDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        //private readonly RoleManager<IdentityRole> _roleManager;

        public ProdutosController(ControleDeLeiloesDbContext context,
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager
        //RoleManager<IdentityRole> roleManager,
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var produto = _context.Produto
                .Include(a => a.Usuario).Where(b => b.Usuario.UserName == User.Identity.Name)
                .ToListAsync();
            return View(await produto);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produto
                .FirstOrDefaultAsync(m => m.Id == id);
            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }


        public IActionResult Create()
        {
            return View();
        }

        // POST: Produtos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Produto produto)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                produto.Usuario = user;
                _context.Add(produto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(produto);
        }

        // GET: Produtos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produto.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }
            return View(produto);
        }

        // POST: Produtos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Produto produto)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProdutoExists(produto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(produto);
        }

        // GET: Produtos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produto
                .FirstOrDefaultAsync(m => m.Id == id);
            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }

        // POST: Produtos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produto = await _context.Produto.FindAsync(id);
            _context.Produto.Remove(produto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProdutoExists(int id)
        {
            return _context.Produto.Any(e => e.Id == id);
        }


        // POST: Produtos/BuscarAnuncio

        public JsonResult BuscarAnuncio(string link)
        {
            string jsonString;
            var produto = new Produto();

            if (ModelState.IsValid)
            {
                if (link != null)
                {
                    //var requisicaoWeb = WebRequest.CreateHttp("https://ma.olx.com.br/regiao-de-sao-luis/celulares/vendo-iphone-6-794456376");
                    //var requisicaoWeb = WebRequest.CreateHttp("https://ma.olx.com.br/regiao-de-sao-luis/celulares/iphone-11-pro-792636771");

                    var requisicaoWeb = WebRequest.CreateHttp(link);

                    requisicaoWeb.Method = "GET";
                    requisicaoWeb.UserAgent = "RequisicaoWebDemo";

                    using (var resposta = requisicaoWeb.GetResponse())
                    {
                        var streamDados = resposta.GetResponseStream();
                        StreamReader reader = new StreamReader(streamDados);
                        object objResponse = reader.ReadToEnd();

                        var texto = objResponse.ToString();
                        int tamanho = 0;

                        //Obter descrição
                        int posicao = texto.IndexOf("og:title") + 19;
                        if (posicao > 0)
                        {
                            tamanho = texto.IndexOf("/><meta", posicao) - 1 - posicao;

                            if (tamanho > 0)
                            {
                                produto.Descricao = texto.Substring(posicao, tamanho);
                            }
                        }

                        //preço
                        posicao = texto.IndexOf("price") + 8;
                        if (posicao > 0)
                        {
                            tamanho = texto.IndexOf("},", posicao) - 1 - posicao;

                            if (tamanho > 0)
                            {
                                produto.VlAnunciado = Int32.Parse(texto.Substring(posicao, tamanho));
                            }
                        }

                        //nome vendedor
                        posicao = texto.IndexOf("sellerName") + 13;
                        tamanho = texto.IndexOf(",", posicao) - 1 - posicao;

                        if (tamanho > 0)
                        {
                            produto.Vendedor = texto.Substring(posicao, tamanho);
                        }


                        //bairro
                        posicao = texto.IndexOf("Bairro<");
                        if (posicao > 0)
                        {
                            posicao = texto.IndexOf("<dd ", posicao);
                            posicao = texto.IndexOf(">", posicao) + 1;
                            tamanho = texto.IndexOf("</dd>", posicao) - posicao;

                            if (tamanho > 0)
                            {
                                produto.Bairro = texto.Substring(posicao, tamanho);
                            }
                        }
                        else
                        {
                            posicao = texto.IndexOf("Município<");
                            if (posicao > 0)
                            {
                                posicao = texto.IndexOf("<dd ", posicao);
                                posicao = texto.IndexOf(">", posicao) + 1;
                                tamanho = texto.IndexOf("</dd>", posicao) - posicao;

                                if (tamanho > 0)
                                {
                                    produto.Bairro = texto.Substring(posicao, tamanho);
                                }
                            }
                        }

                        //telefone
                        posicao = texto.IndexOf(";phone") + 38;
                        if (posicao > 0)
                        {
                            tamanho = texto.IndexOf("&quot", posicao) - posicao;

                            if (tamanho > 0)
                            {
                                produto.Telefone = texto.Substring(posicao, tamanho);
                            }
                        }

                        produto.Anuncio = link;
                    }
                }
            }
            jsonString = JsonSerializer.Serialize(produto);
            return Json(jsonString);

        }
    }
}
