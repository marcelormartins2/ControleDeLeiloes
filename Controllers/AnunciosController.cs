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
using System.Threading;
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

        static readonly WebRequest s_WebRequest = WebRequest.CreateHttp("https://df.olx.com.br/para-a-sua-casa?sf=1");
        static readonly List<String> texto = new List<string>();
        static readonly int qntPaginas = 10;

        //ProgressBar
        private static Progresso Progresso = new Progresso { RegistrosBDAnalisados = -1 };
        public JsonResult GetProgresso()
        {
            return Json(Progresso);
        }
        //reseta a clasee progresso
        public JsonResult ResetProgresso()
        {
            Progresso = new Progresso();
            return Json(Progresso);
        }
        // GET: Anuncios
        public async Task<IActionResult> Index(bool? verNotView)
        {
            if (verNotView == null || verNotView == false)
            {
                ViewData["verNotView"] = false;
                return View(await _context.Anuncio.Where(m => m.NotView == false).ToListAsync());
            }
            else
            {
                ViewData["verNotView"] = true;
                return View(await _context.Anuncio.ToListAsync());
            }
        }
        //POST: Atualizar NotView em anuncio
        public async Task<bool> UpdateNotView(bool notView, int id)
        {
            var anuncio = await _context.Anuncio.FindAsync(id);
            if (anuncio == null)
            {
                return false;
            }
            anuncio.NotView = notView;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(anuncio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return false;
                }
            }
            return true;
        }
        // GET: Anuncios/EliminarAnunciosInvalidos
        public IActionResult EliminarAnunciosInvalidos()
        {
            return View();
        }
        //// POST: Anuncios/EliminarAnunciosInvalidos
        //[HttpPost, ActionName("EliminarAnunciosInvalidos")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EliminarAnunciosInvalidosConfirmed()
        //{
        //    DeletarInvalidos();
        //    return null;
        //}
        public async Task<bool> DeletarInvalidos()
        {
            List<Anuncio> listAnuncios = await _context.Anuncio.ToListAsync();

            Progresso.RegistrosBD = listAnuncios.Count();
            Progresso.RegistrosBDApagados = 0;
            Progresso.RegistrosBDAnalisados = 0;

            WebResponse resposta = null;

            foreach (Anuncio item in listAnuncios)
            {
                //Verifica se os anuncios estão ativos
                try
                {
                    WebRequest requisicaoWeb = WebRequest.Create(item.Link);

                    requisicaoWeb.Method = "HEAD";
                    //requisicaoWeb.UserAgent = "RequisicaoWebDemo";

                    resposta = await requisicaoWeb.GetResponseAsync();
                }
                catch (WebException e)
                {
                    if (e.Message.ToString() == "The remote server returned an error: (404) Not Found.")
                    {
                        Progresso.RegistrosBDApagados++;
                        var anuncioDeletar = await _context.Anuncio.FindAsync(item.Id);
                        _context.Anuncio.Remove(anuncioDeletar);
                        await _context.SaveChangesAsync();
                    }
                }
                finally
                {
                    if (resposta != null)
                    {
                        resposta.Close();
                    }
                }
                Progresso.RegistrosBDAnalisados++;
            }
            //Progresso.RegistrosBD = 0;
            return true;
        }
        // GET: Anuncios/Create
        public IActionResult Create()
        {
            return View();
        }
        //Atualizar anuncios
        public async Task<bool> AtualizarAnuncios()
        {
            if (Progresso.EtapaAnuncio == 0 && Progresso.EtapaPagina == 0)
            {
                //barra de progresso
                //string[] texto = new string[qntPaginas];
                Progresso.QuantidadePaginas = qntPaginas;
                Progresso.EtapaPagina = 1;
                Progresso.Estagio = "Páginas";
                Progresso.Gravado = false;

                string texto2 = "";

                int posicao = 0;
                int posicao1 = 0;
                int posicaoInicial = 0;
                int tamanho = 0;
                int x = 0;
                int qntAnuncios = 0;
                var anuncios = new List<Anuncio>();
                var anuncioUnico = new Anuncio();
                List<String> listPaginas = new List<string>();

                WebResponse resposta;
                WebResponse resposta2;
                Stream streamDados;
                StreamReader reader;
                object objResponse;

                //criar lista de páginas
                string link = "https://df.olx.com.br/para-a-sua-casa?sf=1";
                listPaginas.Add(link);
                for (int i = 2; i <= qntPaginas; i++)
                {
                    link = "https://df.olx.com.br/para-a-sua-casa?o=" + i + "&sf=1";
                    listPaginas.Add(link);
                }

                //baixar páginas
                await getPaginas(listPaginas);
                //Total de anuncios
                foreach (String item in texto)
                {
                    x = 0;
                    while (item.IndexOf("data-lurker_list_id", x) > -1)
                    {
                        x = item.IndexOf("data-lurker_list_id", x);
                        if (x > -1)
                        {
                            x++;
                            qntAnuncios++;
                        }
                    }
                }

                //BUSCAR ANUNCIOS
                //barra de progresso
                //qntAnuncios = 10; // limitar a quantidade de anuncios
                Progresso.QuantidadeAnuncios = qntAnuncios;
                Progresso.EtapaAnuncio = 0;
                Progresso.Estagio = "Anuncios";

                //Busca o Id do último último anuncio cadastrado no banco de dados
                Anuncio novoAnuncio = await _context.Anuncio.OrderByDescending(o => o.Id).FirstOrDefaultAsync();
                int lastId = (novoAnuncio != null) ? novoAnuncio.Id : 0;
                int posAnt;

                try
                {
                    foreach (String item in texto)
                    {
                        posicaoInicial = 0;
                        posicao = 0;
                        posAnt = 0;
                        posicao1 = 0;

                        while (item.IndexOf("data-lurker_list_id", posicaoInicial) > -1 && qntAnuncios > 0)
                        {
                            posicaoInicial = item.IndexOf("data-lurker_list_id", posicaoInicial) + 10;

                            qntAnuncios--;

                            //IdAnuncio
                            posAnt = posicao;
                            posicao = item.IndexOf("data-lurker_list_id", posicao) + 21;
                            tamanho = item.IndexOf("data-lurker", posicao) - 2 - posicao;
                            if (tamanho > 0)
                            {
                                anuncioUnico.IdAnuncio = item.Substring(posicao, tamanho);
                            }

                            //verifica se o anuncio já está cadastrado
                            if (_context.Anuncio.FirstOrDefault(o => o.IdAnuncio == anuncioUnico.IdAnuncio) != null)
                            {
                                Progresso.QuantidadeAnuncios--;
                            }
                            else
                            {
                                //Link
                                posicao = item.IndexOf("href=\"https://df.olx.com.br", posicao) + 6;
                                tamanho = item.IndexOf(" target=", posicao) - 1 - posicao;
                                if (tamanho > 0)
                                {
                                    Uri uriResult;
                                    string uriTmp = item.Substring(posicao, tamanho);
                                    bool result = Uri.TryCreate(uriTmp, UriKind.Absolute, out uriResult)
                                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                                    if (result)
                                    {
                                        anuncioUnico.Link = uriTmp;
                                    }
                                }

                                //acessr Link
                                //nova página
                                if (anuncioUnico.Link.Length < 2083)
                                {

                                    var requisicaoWeb = WebRequest.CreateHttp(anuncioUnico.Link);
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

                                        //    if (int.Parse(item.Substring(posicao2, tamanho)) > 20)
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
                                            "da fabrica",
                                            "da fábrica",
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
                                            Progresso.QuantidadeAnuncios--;
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
                                                    Uri uriResult;
                                                    string uriTmp = texto2.Substring(posicao1, tamanho);
                                                    bool result = Uri.TryCreate(uriTmp, UriKind.Absolute, out uriResult)
                                                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                                                    if (result)
                                                    {
                                                        switch (count)
                                                        {
                                                            case 1:
                                                                anuncioUnico.Img1 = uriTmp;
                                                                break;
                                                            case 2:
                                                                anuncioUnico.Img2 = uriTmp;
                                                                break;
                                                            case 3:
                                                                anuncioUnico.Img3 = uriTmp;
                                                                break;
                                                        }
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
                                            tamanho = texto2.IndexOf(",", posicao1) - 1 - posicao1;
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

                                            // GRAVAÇÃO DE REGISTRO
                                            // barra de progressão

                                            //Progresso.Estagio = "Gravando Anuncios";

                                            //Busca o último Id classificando em ordem descendente, pois LastOrDefault não funciona
                                            if (ModelState.IsValid)
                                            {
                                                anuncioUnico.Id = (lastId > 0) ? lastId + 1 : 1;
                                                _context.Add(anuncioUnico);
                                                await _context.SaveChangesAsync();
                                                lastId++;
                                                anuncioUnico = new Anuncio();
                                            }

                                            //Barra de Progresso
                                            Progresso.EtapaAnuncio++;
                                        }
                                    }
                                }
                            }
                        }

                    }
                    Progresso.Gravado = true;
                    Progresso.EtapaAnuncio = 0;
                    Progresso.EtapaPagina = 0;
                }
                catch (Exception e)
                {
                    Progresso.MensagemErro = e.Message + " / " + e.StackTrace;
                    throw;
                }

                //Progresso.Estagio = "";
                return true;
            }
            return true;
        }

        private async Task getPaginas(List<string> listPaginas)
        {
            IEnumerable<Task<int>> downloadTasksQuery =
               from url in listPaginas
               select ProcessUrlAsync(url, s_WebRequest);

            List<Task<int>> downloadTasks = downloadTasksQuery.ToList();

            while (downloadTasks.Any())
            {
                Task<int> finishedTask = await Task.WhenAny(downloadTasks);
                downloadTasks.Remove(finishedTask);
                if (Progresso.EtapaPagina < qntPaginas )
                {
                    Progresso.EtapaPagina++;
                }
            }
        }

        private async Task<int> ProcessUrlAsync(string url, WebRequest s_webRequest)
        {
            try
            {
                s_webRequest = WebRequest.CreateHttp(url);
                s_webRequest.Method = "GET";
                //s_webRequest.UserAgent = "RequisicaoWebDemo";

                var resposta = await s_webRequest.GetResponseAsync();
                var streamDados = resposta.GetResponseStream();
                var reader = new StreamReader(streamDados);
                var objResponse = reader.ReadToEnd();

                texto.Add(objResponse.ToString());
            }
            catch (WebException e)
            {
                Progresso.MensagemErro = e.Message + " //StackTrace// " + e.StackTrace;
            }
            return 0;
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
