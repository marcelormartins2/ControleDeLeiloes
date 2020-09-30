using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControleDeLeiloes.Data;
using ControleDeLeiloes.Models;
using System.Net;
using System.IO;
using System.Globalization;

namespace ControleDeLeiloes.Controllers
{
    public class AnunciosController : Controller
    {
        private readonly ControleDeLeiloesDbContext _context;

        public AnunciosController(ControleDeLeiloesDbContext context)
        {
            _context = context;
        }

        // GET: Anuncios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Anuncio.ToListAsync());
        }

        // GET: Anuncios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anuncio = await _context.Anuncio
                .FirstOrDefaultAsync(m => m.Id == id);
            if (anuncio == null)
            {
                return NotFound();
            }

            return View(anuncio);
        }

        // GET: Anuncios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Anuncios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Anuncio? anuncioExt)
        {

            var anuncios = new List<Anuncio>();
            var anuncioUnico = new Anuncio();
            string texto2 = "";
            string link = "https://df.olx.com.br/para-a-sua-casa";

            var requisicaoWeb = WebRequest.CreateHttp(link);
            int posicao = 0;

            requisicaoWeb.Method = "GET";
            requisicaoWeb.UserAgent = "RequisicaoWebDemo";

            using (var resposta = await requisicaoWeb.GetResponseAsync())
            {
                var streamDados = resposta.GetResponseStream();
                StreamReader reader = new StreamReader(streamDados);
                object objResponse = reader.ReadToEnd();

                var texto = objResponse.ToString();
                int tamanho = 0;

                WebResponse resposta2;
                //Obter anuncios
                while (texto.IndexOf("data-lurker_list_id", posicao) > -1)
                {
                    //IdAnuncio
                    posicao = texto.IndexOf("data-lurker_list_id") + 21;
                    tamanho = texto.IndexOf("data-lurker", posicao) - 1 - posicao;
                    if (tamanho > 0)
                    {
                        anuncioUnico.IdAnuncio = int.Parse(texto.Substring(posicao, tamanho));
                    }
                }
                //Link
                posicao = texto.IndexOf("data-lurker_list_position", posicao) + 33;
                tamanho = texto.IndexOf(" target=", posicao) - 1 - posicao;
                if (tamanho > 0)
                {
                    anuncioUnico.Link = texto.Substring(posicao, tamanho);
                }
                //acessr Link
                requisicaoWeb = WebRequest.CreateHttp(anuncioUnico.Link);

                resposta2 = await requisicaoWeb.GetResponseAsync();
                streamDados = resposta2.GetResponseStream();
                reader = new StreamReader(streamDados);
                objResponse = reader.ReadToEnd();
                texto2 = objResponse.ToString();

                posicao = 0;
                //IdVendedor
                posicao = texto2.IndexOf("olx.com.br/perfil/", posicao) + 33;
                tamanho = texto2.IndexOf("class=", posicao) - 1;
                posicao = texto2.LastIndexOf("-", posicao) + 1;
                tamanho -= posicao;
                if (tamanho > 0)
                {
                    anuncioUnico.IdVendedor = texto2.Substring(posicao, tamanho);
                }
                //nome vendedor
                posicao = texto2.IndexOf("sellerName") + 13;
                tamanho = texto2.IndexOf(",", posicao) - 1 - posicao;

                if (tamanho > 0)
                {
                    anuncioUnico.Vendedor = texto2.Substring(posicao, tamanho);
                }
                //quantidade de anuncios deste vendedor
                requisicaoWeb = WebRequest
                    .CreateHttp("https://www.olx.com.br/perfil/"
                    + anuncioUnico.Vendedor
                    + "-"
                    + anuncioUnico.IdVendedor);

                using (var resposta3 = await requisicaoWeb.GetResponseAsync())
                {
                    streamDados = resposta.GetResponseStream();
                    reader = new StreamReader(streamDados);
                    objResponse = reader.ReadToEnd();

                    var texto3 = objResponse.ToString();
                    //vendedor desde
                    posicao = texto3.IndexOf("na OLX desde") + 13;
                    tamanho = texto3.IndexOf(" de ", posicao); // + 8 - posicao;

                    if (tamanho > 0)
                    {
                        var mes = DateTime.ParseExact(texto3.Substring(posicao, tamanho), "Y", CultureInfo.InvariantCulture);
                        var ano = DateTime.ParseExact(texto3.Substring(posicao, tamanho), "yyyy", CultureInfo.InvariantCulture);
                        anuncioUnico.DtVendedorDesde = DateTime.ParseExact(mes + "/" + ano, "mm/yyyy", CultureInfo.InvariantCulture);
                    }

                    //quantidade de anuncios
                    posicao = texto3.IndexOf("AdCount__StyledCount-sc-1yney30-0 bwqcMo");
                    posicao = texto3.IndexOf("span") + 5;

                    tamanho = texto3.IndexOf("/span", posicao) - 1 - posicao;

                    if (int.Parse(texto.Substring(posicao, tamanho)) > 20)
                    {
                        //cadastra vendedorProibido
                        var vendedorProibido = new VendedorProibido();
                        vendedorProibido.IdVendedor = anuncioUnico.IdVendedor;
                        vendedorProibido.Nome = anuncioUnico.Vendedor;

                        _context.Add(vendedorProibido);
                        await _context.SaveChangesAsync();
                    }

                }

                //Verifica se o vendedor é proíbido
                if (_context.VendedorProibido.FirstOrDefaultAsync(v => v.IdVendedor == anuncioUnico.IdVendedor) == null)
                {
                    var excludentes = new List<string> {
                        "anúncio profissional",
                        "a partir de ",
                        "Dividimos em até ",
                        "frete grátis",
                        "diretamente da fábrica"};
                    int count = 0;
                    foreach (string element in excludentes)
                    {
                        count++;
                        if (texto2.IndexOf(element) > -1)
                        {
                            //cadastra vendedorProibido
                            var vendedorProibido = new VendedorProibido();
                            vendedorProibido.IdVendedor = anuncioUnico.IdVendedor;
                            vendedorProibido.Nome = anuncioUnico.Vendedor;

                            _context.Add(vendedorProibido);
                            await _context.SaveChangesAsync();
                            break;
                        }
                    }
                    //imagens
                    posicao = texto2.IndexOf("lkx530-4 hXBoAC");
                    count = 0;
                    while (posicao > -1 && count < 2)
                    {
                        count++;
                        posicao += 26;
                        tamanho = texto2.IndexOf("alt=", posicao) - 1 - posicao;
                        if (tamanho > 0)
                        {
                            switch (count)
                            {
                                case 1:
                                    anuncioUnico.Img1 = texto2.Substring(posicao, tamanho);
                                    break;
                                case 2:
                                    anuncioUnico.Img2 = texto2.Substring(posicao, tamanho);
                                    break;
                                case 3:
                                    anuncioUnico.Img3 = texto2.Substring(posicao, tamanho);
                                    break;
                            }
                        }

                    }
                    //Titulo
                    posicao = texto2.IndexOf("sc-1sj3nln-1 eOSweo sc-ifAKCX cmFKIN");
                    posicao = texto2.IndexOf("weight") + 11;
                    tamanho = texto2.IndexOf("/span", posicao) - 1 - posicao;
                    if (tamanho > 0)
                    {
                        anuncioUnico.Titulo = texto2.Substring(posicao, tamanho);
                    }
                    //Descrição
                    posicao = texto2.IndexOf("og:title");
                    if (posicao > 0)
                    {
                        posicao += 19;
                        tamanho = texto2.IndexOf("/><meta", posicao) - 1 - posicao;

                        if (tamanho > 0)
                        {
                            anuncioUnico.Descricao = texto2.Substring(posicao, tamanho);
                        }
                    }
                    //Data Publicação
                    posicao = texto2.IndexOf("Publicado em <") + 22;
                    tamanho = 5;
                    anuncioUnico.DtPublicacao = DateTime.ParseExact(texto2.Substring(posicao, tamanho), "M/MM", CultureInfo.InvariantCulture);
                    //preço
                    posicao = texto2.IndexOf("price") + 8;
                    tamanho = texto2.IndexOf("},", posicao) - 1 - posicao;
                    if (tamanho > 0)
                    {
                        anuncioUnico.VlAnunciado = Int32.Parse(texto2.Substring(posicao, tamanho));
                    }
                    //bairro
                    posicao = texto2.IndexOf("Bairro<");
                    if (posicao > 0)
                    {
                        posicao = texto2.IndexOf("<dd ", posicao);
                        posicao = texto2.IndexOf(">", posicao) + 1;
                        tamanho = texto2.IndexOf("</dd>", posicao) - posicao;
                        if (tamanho > 0)
                        {
                            anuncioUnico.Bairro = texto2.Substring(posicao, tamanho);
                        }
                    }
                    else
                    {
                        posicao = texto2.IndexOf("Município<");
                        if (posicao > 0)
                        {
                            posicao = texto2.IndexOf("<dd ", posicao);
                            posicao = texto2.IndexOf(">", posicao) + 1;
                            tamanho = texto2.IndexOf("</dd>", posicao) - posicao;

                            if (tamanho > 0)
                            {
                                anuncioUnico.Bairro = texto2.Substring(posicao, tamanho);
                            }
                        }
                    }
                    //telefone
                    posicao = texto2.IndexOf(";phone") + 38;
                    tamanho = texto2.IndexOf("&quot", posicao) - posicao;

                    if (tamanho > 0)
                    {
                        anuncioUnico.Telefone = texto2.Substring(posicao, tamanho);
                    }
                    anuncios.Add(anuncioUnico);
                }
                if (ModelState.IsValid)
                {
                    foreach (Anuncio item in anuncios)
                    {
                        _context.Add(item);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: Anuncios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anuncio = await _context.Anuncio.FindAsync(id);
            if (anuncio == null)
            {
                return NotFound();
            }
            return View(anuncio);
        }

        // POST: Anuncios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdAnuncio,Link,DtPublicacao,Img1,Img2,Img3,Descricao,VlAnunciado,Bairro,Telefone,Vendedor,IdVendedor,DtVendedorDesde")] Anuncio anuncio)
        {
            if (id != anuncio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(anuncio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnuncioExists(anuncio.Id))
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
            return View(anuncio);
        }

        // GET: Anuncios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anuncio = await _context.Anuncio
                .FirstOrDefaultAsync(m => m.Id == id);
            if (anuncio == null)
            {
                return NotFound();
            }

            return View(anuncio);
        }

        // POST: Anuncios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var anuncio = await _context.Anuncio.FindAsync(id);
            _context.Anuncio.Remove(anuncio);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnuncioExists(int id)
        {
            return _context.Anuncio.Any(e => e.Id == id);
        }
    }
}
