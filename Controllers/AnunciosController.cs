using ControleDeLeiloes.Data;
using ControleDeLeiloes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

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

        //ProgressBar
        private static Progresso Progresso = new Progresso();
        public JsonResult GetProgress()
        {
            return Json(Progresso);
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

        //Atualizar anuncios
        struct AnuncioUnico
        {
            public double X, Y;
            public override string ToString()
            {
                return "(" + X + "," + Y + ")";
            }
        }
        public async Task<bool> AtualizarAnuncios()
        {
            if (Progresso.Etapa < 1)
            {
                string texto2 = "";

                int posicao = 0;
                int posicao1 = 0;
                int posicaoInicial = 0;
                int tamanho = 0;
                int x = 0;
                int qntAnuncios = 0;
                int qntPaginas = 3;
                var anuncios = new List<Anuncio>();
                var anuncioUnico = new Anuncio();

                Progresso.Quantidade = 1;

                string link = "https://df.olx.com.br/para-a-sua-casa";
                var requisicaoWeb = WebRequest.CreateHttp(link);
                WebResponse resposta;
                WebResponse resposta2;
                Stream streamDados;
                StreamReader reader;
                object objResponse;

                string[] texto = new string[qntPaginas];
                Progresso.Quantidade = qntPaginas;
                Progresso.Etapa = 0;
                Progresso.Estagio = "Buscando Páginas";

                for (int i = 0; i < qntPaginas; i++)
                {
                    posicao = 0;
                    posicao1 = 0;
                    posicaoInicial = 0;
                    tamanho = 0;
                    x = 0;
                    Progresso.Etapa++;

                    if (i > 0)
                    {
                        link = "https://df.olx.com.br/para-a-sua-casa?o=" + i + 1;
                        requisicaoWeb = WebRequest.CreateHttp(link);
                    }

                    requisicaoWeb.Method = "GET";
                    requisicaoWeb.UserAgent = "RequisicaoWebDemo";

                    resposta = await requisicaoWeb.GetResponseAsync();
                    streamDados = resposta.GetResponseStream();
                    reader = new StreamReader(streamDados);
                    objResponse = reader.ReadToEnd();

                    texto[i] = objResponse.ToString();

                    while (texto[i].IndexOf("data-lurker_list_id", x) > -1)
                    {
                        x = texto[i].IndexOf("data-lurker_list_id", x) + 1;
                        qntAnuncios++;
                    }
                }

                //barra de progresso
                Progresso.Quantidade = qntAnuncios;
                Progresso.Etapa = 0;
                Progresso.Estagio = "Anuncios";

                for (int i = 0; i < qntPaginas; i++)
                {
                    //Obter anuncios
                    while (texto[i].IndexOf("data-lurker_list_id", posicaoInicial) > -1 && qntAnuncios > 0)
                    {
                        posicaoInicial = texto[i].IndexOf("data-lurker_list_id", posicaoInicial) + 10;

                        qntAnuncios--;
                        //IdAnuncio
                        posicao = texto[i].IndexOf("data-lurker_list_id", posicao) + 21;
                        tamanho = texto[i].IndexOf("data-lurker", posicao) - 2 - posicao;
                        if (tamanho > 0)
                        {
                            anuncioUnico.IdAnuncio = texto[i].Substring(posicao, tamanho);
                        }

                        //verifica se o anuncio já está cadastrado
                        if (_context.Anuncio.FirstOrDefault(o => o.IdAnuncio == anuncioUnico.IdAnuncio) != null)
                        {
                            Progresso.Quantidade--;
                        }
                        else
                        {
                            //Link
                            posicao = texto[i].IndexOf("href", posicao) + 6;
                            tamanho = texto[i].IndexOf(" target=", posicao) - 1 - posicao;
                            if (tamanho > 0)
                            {
                                var txt = texto[i].Substring(posicao, tamanho);
                                anuncioUnico.Link = txt;
                            }

                            //acessr Link
                            //nova página
                            requisicaoWeb = WebRequest.CreateHttp(anuncioUnico.Link);
                            using (resposta2 = await requisicaoWeb.GetResponseAsync())
                            {
                                //resposta2 = requisicaoWeb.GetResponse();
                                streamDados = resposta2.GetResponseStream();
                                reader = new StreamReader(streamDados);
                                objResponse = reader.ReadToEnd();
                                texto2 = objResponse.ToString();

                                posicao1 = 0;
                                ////IdVendedor
                                //posicao1 = texto2.IndexOf("olx.com.br/perfil/", posicao1) + 33;
                                //tamanho = texto2.IndexOf("class=", posicao1) - 1;
                                //posicao1 = texto2.LastIndexOf("-", posicao1) + 1;
                                //tamanho -= posicao1;
                                //if (tamanho > 0)
                                //{
                                //    anuncioUnico.IdVendedor = texto2.Substring(posicao1, tamanho);
                                //}
                                //nome vendedor
                                posicao1 = texto2.IndexOf("sellerName") + 13;
                                tamanho = texto2.IndexOf(",", posicao1) - 1 - posicao1;

                                if (tamanho > 0)
                                {
                                    anuncioUnico.Vendedor = texto2.Substring(posicao1, tamanho);
                                }

                                ////acessr Link
                                ////nova página
                                ////quantidade de anuncios deste vendedor
                                //requisicaoWeb = WebRequest
                                //    .CreateHttp("https://www.olx.com.br/perfil/"
                                //    + anuncioUnico.Vendedor
                                //    + "-"
                                //    + anuncioUnico.IdVendedor);
                                //posicao2 = 0;
                                //using (var resposta3 = await requisicaoWeb.GetResponseAsync())
                                //{
                                //    streamDados = resposta.GetResponseStream();
                                //    reader = new StreamReader(streamDados);
                                //    objResponse = reader.ReadToEnd();

                                //    var texto3 = objResponse.ToString();
                                //    //vendedor desde
                                //    posicao2 = texto3.IndexOf("na OLX desde") + 13;
                                //    tamanho = texto3.IndexOf(" de ", posicao2); // + 8 - posicao2;

                                //    if (tamanho > 0)
                                //    {
                                //        var mes = DateTime.ParseExact(texto3.Substring(posicao2, tamanho), "Y", CultureInfo.InvariantCulture);
                                //        var ano = DateTime.ParseExact(texto3.Substring(posicao2, tamanho), "yyyy", CultureInfo.InvariantCulture);
                                //        anuncioUnico.DtVendedorDesde = DateTime.ParseExact(mes + "/" + ano, "mm/yyyy", CultureInfo.InvariantCulture);
                                //    }

                                //    //quantidade de anuncios
                                //    posicao2 = texto3.IndexOf("AdCount__StyledCount-sc-1yney30-0 bwqcMo");
                                //    posicao2 = texto3.IndexOf("span") + 5;

                                //    tamanho = texto3.IndexOf("/span", posicao2) - 1 - posicao2;

                                //    if (int.Parse(texto[i].Substring(posicao2, tamanho)) > 20)
                                //    {
                                //        //cadastra vendedorProibido
                                //        var vendedorProibido = new VendedorProibido();
                                //        vendedorProibido.IdVendedor = anuncioUnico.IdVendedor;
                                //        vendedorProibido.Nome = anuncioUnico.Vendedor;

                                //        _context.Add(vendedorProibido);
                                //        await _context.SaveChangesAsync();
                                //    }

                                //}

                                //Verifica se o vendedor é proíbido
                                //if (_context.VendedorProibido.FirstOrDefaultAsync(v => v.IdVendedor == anuncioUnico.IdVendedor) == null)
                                //{
                                var excludentes = new List<string>
                                {
                                    "anúncio profissional",
                                    "a partir de ",
                                    "dividimos em até ",
                                    "dividimos em ate ",
                                    "frete grátis",
                                    "frete gratis",
                                    "direto da fábrica",
                                    "direto da fabrica",
                                    "em promoção",
                                    "em promocao",
                                    "diretamente da fabrica",
                                    "diretamente da fábrica"
                                };
                                int count = 0;
                                Boolean vendedorProibido = false;
                                foreach (string element in excludentes)
                                {
                                    count++;
                                    if (texto2.IndexOf(element, StringComparison.CurrentCultureIgnoreCase) > -1)
                                    {
                                        vendedorProibido = true;
                                    }
                                }
                                if (vendedorProibido)
                                {
                                    //decrementa quantidade na barra de progresso
                                    Progresso.Quantidade--;
                                }
                                else
                                {
                                    //imagens
                                    posicao1 = texto2.IndexOf("lkx530-4 hXBoAC");
                                    count = 0;
                                    while (posicao1 > -1 && count < 3)
                                    {
                                        count++;
                                        posicao1 += 27;
                                        tamanho = texto2.IndexOf("alt=", posicao1) - 2 - posicao1;
                                        if (tamanho > 0)
                                        {
                                            switch (count)
                                            {
                                                case 1:
                                                    anuncioUnico.Img1 = texto2.Substring(posicao1, tamanho);
                                                    break;
                                                case 2:
                                                    anuncioUnico.Img2 = texto2.Substring(posicao1, tamanho);
                                                    break;
                                                case 3:
                                                    anuncioUnico.Img3 = texto2.Substring(posicao1, tamanho);
                                                    break;
                                            }
                                        }
                                        posicao1 = texto2.IndexOf("lkx530-4 hXBoAC", posicao1);
                                    }
                                    //Descricao
                                    posicao1 = texto2.IndexOf("sc-1sj3nln-1 eOSweo sc-ifAKCX cmFKIN");
                                    posicao1 = texto2.IndexOf("weight", posicao1) + 13;
                                    tamanho = texto2.IndexOf("/span", posicao1) - 1 - posicao1;
                                    if (tamanho > 0)
                                    {
                                        anuncioUnico.Descricao = texto2.Substring(posicao1, tamanho);
                                    }
                                    //Titulo
                                    posicao1 = texto2.IndexOf("og:title");
                                    if (posicao1 > 0)
                                    {
                                        posicao1 += 19;
                                        tamanho = texto2.IndexOf("/><meta", posicao1) - 1 - posicao1;

                                        if (tamanho > 0)
                                        {
                                            anuncioUnico.Titulo = texto2.Substring(posicao1, tamanho);
                                        }
                                    }
                                    //Data Publicação
                                    posicao1 = texto2.IndexOf("Publicado em <") + 21;
                                    tamanho = 5;
                                    anuncioUnico.DtPublicacao = DateTime.ParseExact(texto2.Substring(posicao1, tamanho), "dd/MM", CultureInfo.InvariantCulture);
                                    //preço
                                    posicao1 = texto2.IndexOf("price") + 8;
                                    tamanho = texto2.IndexOf("},", posicao1) - 1 - posicao1;
                                    if (tamanho > 0)
                                    {
                                        anuncioUnico.VlAnunciado = Int32.Parse(texto2.Substring(posicao1, tamanho));
                                    }
                                    //bairro
                                    posicao1 = texto2.IndexOf("Bairro<");
                                    if (posicao1 > 0)
                                    {
                                        posicao1 = texto2.IndexOf("<dd ", posicao1);
                                        posicao1 = texto2.IndexOf(">", posicao1) + 1;
                                        tamanho = texto2.IndexOf("</dd>", posicao1) - posicao1;
                                        if (tamanho > 0)
                                        {
                                            anuncioUnico.Bairro = texto2.Substring(posicao1, tamanho);
                                        }
                                    }
                                    else
                                    {
                                        posicao1 = texto2.IndexOf("Município<");
                                        if (posicao1 > 0)
                                        {
                                            posicao1 = texto2.IndexOf("<dd ", posicao1);
                                            posicao1 = texto2.IndexOf(">", posicao1) + 1;
                                            tamanho = texto2.IndexOf("</dd>", posicao1) - posicao1;

                                            if (tamanho > 0)
                                            {
                                                anuncioUnico.Bairro = texto2.Substring(posicao1, tamanho);
                                            }
                                        }
                                    }
                                    //telefone
                                    posicao1 = texto2.IndexOf(";phone") + 38;
                                    tamanho = texto2.IndexOf("&quot", posicao1) - posicao1;

                                    if (tamanho > 0)
                                    {
                                        anuncioUnico.Telefone = texto2.Substring(posicao1, tamanho);
                                    }

                                    anuncios.Add(anuncioUnico.Clone());

                                    //Barra de Progresso
                                    Progresso.Etapa++;
                                }
                            }
                        }
                    }

                    //cadastra anuncios no banco de dados

                    if (ModelState.IsValid)
                    {
                        Anuncio novoAnuncio = await _context.Anuncio.OrderByDescending(o => o.Id).FirstOrDefaultAsync();
                        //Anuncio novoAnuncio = _context.Anuncio.OrderByDescending(o => o.Id).FirstOrDefault();
                        int lastId = 0;
                        if (novoAnuncio != null)
                        {
                            lastId = novoAnuncio.Id;
                        }

                        foreach (Anuncio item in anuncios)
                        {
                            if (lastId > 0)
                            {
                                item.Id = lastId + 1;
                            }
                            else
                            {
                                item.Id = 1;
                            }

                            _context.Add(item);
                            await _context.SaveChangesAsync();
                            //_context.SaveChanges();
                            lastId++;
                        }
                        //return RedirectToAction(nameof(Index));
                        Progresso.Etapa = 0;
                        return true;
                    }

                }
            }
            //return RedirectToAction(nameof(Index));

            Progresso.Etapa = 0;
            return true;
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
