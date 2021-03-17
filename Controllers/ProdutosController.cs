using ControleDeLeiloes.Data;
using ControleDeLeiloes.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ControleDeLeiloes.Controllers
{

    public class ProdutosController : Controller
    {

        private static readonly HttpClient clienteHttp = new HttpClient();
        //private static readonly HttpClientHandler handler = new HttpClientHandler( HttpClient();
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
        public async Task<IActionResult> ImportarBsb()
        {
            //Buscar dados do site BSBLeilões

            return View();
        }
        public async Task<IActionResult> BuscarLotesBsb(string username, string password)
        {
            if (username == null)
            {
                return RedirectToAction("ImportarBsb", "Produtos");
            }
            try
            {
                CookieContainer cookieJar = new CookieContainer();
                var client = new RestClient("https://www.bsbleiloes.com.br/arrematante/login");
                client.CookieContainer = cookieJar;
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("_password", password);
                request.AddParameter("_username", username);
                var response = client.Execute(request);
                client = new RestClient("https://www.bsbleiloes.com.br/arrematante/minhas-arrematacoes");
                client.CookieContainer = cookieJar;
                client.Timeout = -1;
                request = new RestRequest(Method.GET);
                response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var textoGeral = response.Content;
                    List<Produto> listProduto = new List<Produto>();
                    int pos1;
                    int pos2;
                    int limite;
                    DateTime dataLeilao;
                    int x = 0;
                    pos1 = textoGeral.LastIndexOf("card card-plain");
                    if (pos1 > -1)
                    {
                        //busca ultimo Id de produto no BD e incrementa-o quando for criar novo produto
                        int lastId = await _context.Produto.OrderByDescending(o => o.Id).Select(o => o.Id).FirstOrDefaultAsync();
                        var usuario = await _userManager.FindByNameAsync(User.Identity.Name);
                        //lastId = (ultimoProduto != null) ? ultimoProduto.Id + 1 : 1;
                        // data do leilao
                        pos1 = textoGeral.IndexOf("aria-controls", pos1);
                        pos1 = textoGeral.IndexOf(">", pos1) + 1;
                        pos2 = textoGeral.IndexOf("<i class=", pos1) - 1;
                        dataLeilao = DateTime.Parse(textoGeral.Substring(pos2 - 104, 10).TrimStart().TrimEnd());
                        //tituloLeilao = "BSB-" + dataLeilao.ToString("dd/MM/yy");
                        //verifica no banco de dados se o primeiro produto já está cadastrado
                        if (await _context.Produto.AnyAsync(m => m.DataAnuncio == dataLeilao && m.Vendedor == "BSB"))
                        {
                            pos1 = -1; //não processa os dados da página
                        }
                        while (pos1 > -1)
                        {
                            limite = textoGeral.IndexOf("</tbody>", pos1);
                            pos1 = textoGeral.IndexOf("<td>", pos1) + 5;
                            while (pos1 < limite && pos1 > 0)
                            {
                                listProduto.Add(new Produto() { Id = ++lastId, UsuarioId = usuario.Id, DataAnuncio = dataLeilao, DataCadastro = DateTime.Now.Date, Vendedor = "BSB" });
                                //lote
                                pos2 = textoGeral.IndexOf("</td>", pos1) - 1;
                                listProduto[x].Descricao += $"Lote {textoGeral.Substring(pos1, pos2 - pos1).Trim()}";
                                //bem
                                pos1 = textoGeral.IndexOf("<td>", pos1 + 1) + 5;
                                pos2 = textoGeral.IndexOf("</td>", pos1) - 1;
                                listProduto[x].Titulo = textoGeral.Substring(pos1, pos2 - pos1).TrimStart().TrimEnd();
                                if (listProduto[x].Titulo.IndexOf("(NO ESTADO)") > 0)
                                {
                                    listProduto[x].Titulo = listProduto[x].Titulo.Remove(listProduto[x].Titulo.IndexOf("(NO ESTADO)"), 11);
                                }
                                //valor pago
                                for (int i = 0; i < 5; i++)
                                {
                                    pos1 = textoGeral.IndexOf("<td>", pos1 + 2);
                                }
                                pos1 = textoGeral.IndexOf("R$", pos1) + 3;
                                pos2 = textoGeral.IndexOf("\n", pos1);
                                double valorTemp = 0;
                                bool result = double.TryParse(textoGeral.Substring(pos1, pos2 - pos1).TrimStart().TrimEnd(), out valorTemp);
                                listProduto[x].VlCompra = valorTemp;
                                ++x;
                                pos1 = textoGeral.IndexOf("<td>", pos1) + 5;
                            }
                            //proximo leilão
                            textoGeral = textoGeral.Substring(0, textoGeral.LastIndexOf("card card-plain") - 1);
                            pos1 = textoGeral.LastIndexOf("card card-plain");
                            // data do leilao
                            pos1 = textoGeral.IndexOf("aria-controls", pos1);
                            pos1 = textoGeral.IndexOf(">", pos1) + 1;
                            pos2 = textoGeral.IndexOf("<i class=", pos1) - 1;
                            dataLeilao = DateTime.Parse(textoGeral.Substring(pos2 - 104, 10).TrimStart().TrimEnd());
                        }
                        if (listProduto.Count > 0)
                        {
                            //incluir produtos no BD
                            _context.Produto.AddRange(listProduto);
                            await _context.SaveChangesAsync();
                            return RedirectToAction("Index", "Produtos");
                        }
                    }
                }
            }
            catch (WebException)
            {
                return RedirectToAction("ImportarBsb", "Produtos");
            }
            return RedirectToAction("ImportarBsb", "Produtos");
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


        public async Task<IActionResult> Create(int? anuncioId)
        {
            if (anuncioId > 0)
            {
                //busca os dados do anuncio para preencher os campos do produto
                Anuncio anuncio = await _context.Anuncio.FirstOrDefaultAsync(m => m.Id == anuncioId);
                Produto produto = new Produto();

                produto.Titulo = anuncio.Titulo;
                produto.Anuncio = anuncio.Link;
                produto.Bairro = anuncio.Bairro;
                produto.DataCadastro = DateTime.Now;
                produto.Descricao = anuncio.Descricao;
                produto.Endereco = anuncio.UF;
                produto.Telefone = anuncio.Telefone;
                produto.Vendedor = anuncio.Vendedor;
                produto.VlAnunciado = anuncio.VlAnunciado;

                //Busca o último Id
                Produto ultimoProduto = await _context.Produto.OrderByDescending(o => o.Id).FirstOrDefaultAsync();
                produto.Id = (ultimoProduto != null) ? ultimoProduto.Id + 1 : 1;

                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                produto.Usuario = user;
                _context.Add(produto);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
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
