using ControleDeLeiloes.Data;
using ControleDeLeiloes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        static List<String> conteudoPaginas = new List<string>();
        static List<String> listaLinksAnuncios = new List<string>();
        static List<String> conteudoAnuncios = new List<string>();
        static List<Anuncio> dadosAnuncios = new List<Anuncio>();
        static List<CategoriaAnuncio> categoriaAnuncios = new List<CategoriaAnuncio>();
        static List<SubcategoriaAnuncio> subcategoriaAnuncios = new List<SubcategoriaAnuncio>();
        static List<CategoriaAnuncio> novasCategoriaAnuncios = new List<CategoriaAnuncio>();
        static List<SubcategoriaAnuncio> novasSubcategoriaAnuncios = new List<SubcategoriaAnuncio>();
        static readonly int qntPaginas = 5;
        static int lastId = 0;
        static int lastCategoriaAnuncioId = 0;
        static int lastSubcategoriaAnuncioId = 0;
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
        public async Task<IActionResult> Index(bool verNotView, int? categoriaId, int? subcategoriaId, bool olxPay=true, bool olxDelivery=true)
        {
            int tempCategoriaId = _context.CategoriaAnuncio.OrderBy(m => m.Nome).FirstOrDefault().Id;
            olxPay = olxDelivery == true ? true : olxPay;
            ViewData["verNotView"] = verNotView;
            ViewBag.olxPay = olxPay;
            ViewBag.olxDelivery = olxDelivery;
            if (categoriaId > 0)
            {
                ViewBag.categoriaId = new SelectList(_context.CategoriaAnuncio.OrderBy(m => m.Nome), "Id", "Nome", categoriaId);
                if (subcategoriaId > 0)
                {
                    ViewBag.subcategoriaId = new SelectList(_context.SubcategoriaAnuncio.Where(m => m.CategoriaAnuncioId == categoriaId).OrderBy(m => m.Nome), "Id", "Nome", subcategoriaId);
                    ViewData["subCategoriaEmpty"] = false;
                }
                else
                {
                    ViewBag.subcategoriaId = new SelectList(_context.SubcategoriaAnuncio.Where(m => m.CategoriaAnuncioId == categoriaId).OrderBy(m => m.Nome), "Id", "Nome");
                    ViewData["subCategoriaEmpty"] = true;
                }
            }
            else
            {
                ViewBag.categoriaId = new SelectList(_context.CategoriaAnuncio.OrderBy(m => m.Nome), "Id", "Nome");
                ViewBag.subcategoriaId = new SelectList(_context.SubcategoriaAnuncio.Where(m => m.CategoriaAnuncioId == tempCategoriaId).OrderBy(m => m.Nome), "Id", "Nome");
                ViewData["subCategoriaEmpty"] = false;
            }

            if ((categoriaId == null || categoriaId == 0) && (subcategoriaId == null || subcategoriaId == 0))
            {
                var tempAnuncio = await _context.Anuncio
                    .Include(m => m.SubcategoriaAnuncio)
                    .Where(m => m.SubcategoriaAnuncio.CategoriaAnuncioId == tempCategoriaId 
                    && m.OlxPay == olxPay
                    && m.OlxDelivery == olxDelivery
                    && m.NotView == verNotView)
                    .AsNoTracking()
                    .ToListAsync();
                //var anuncio = await _context.Anuncio.Include(b=>b.SubcategoriaAnuncio).Where(m =>
                //    m.NotView == verNotView &&
                //    m.OlxPay == olxPay &&
                //    m.OlxDelivery == olxDelivery &&
                //    m.SubcategoriaAnuncio.CategoriaAnuncioId.Equals(2)).AsNoTracking().ToListAsync();
                //anuncio.Where("SubcategoriaAnuncio.CategoriaAnuncioId == tempCategoriaId").ToList();
                return View(tempAnuncio);
            }
            else if (categoriaId != null && categoriaId != 0)
            {
                if (subcategoriaId != null && subcategoriaId != 0)
                {
                    var tempAnuncio = await _context.Anuncio
                        .Where(m => m.SubcategoriaAnuncioId == subcategoriaId 
                        && m.OlxPay == olxPay 
                        && m.OlxDelivery == olxDelivery 
                        && m.NotView == verNotView)
                        .AsNoTracking()
                        .ToListAsync();
                    return View(tempAnuncio);
                }
                else
                {
                    var tempanuncio = await _context.Anuncio
                        .Include(m => m.SubcategoriaAnuncio)
                        .Where(m => m.SubcategoriaAnuncio.CategoriaAnuncioId == categoriaId
                        && m.OlxPay == olxPay
                        && m.OlxDelivery == olxDelivery
                        && m.NotView == verNotView)
                        .AsNoTracking()
                        .ToListAsync();
                    return View(tempanuncio);
                    //m.NotView == verNotView &&
                    //m.OlxPay == olxPay &&
                    //m.OlxDelivery == olxDelivery //&&
                    //                             //m.CategoriaAnuncioId == categoria
                    //);.ToListAsync());
                }
            }
            else
            {
                var tempAnuncio = await _context.Anuncio
                    .Where(m => m.SubcategoriaAnuncioId == subcategoriaId
                    && m.OlxPay == olxPay
                    && m.OlxDelivery == olxDelivery
                    && m.NotView == verNotView)
                    .AsNoTracking()
                    .ToListAsync();
                return View(tempAnuncio);
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
        // POST: Subcategoria
        public JsonResult PostSubcategoria(int categoriaId)
        {
            if (categoriaId > 0)
            {
                var tempSub = _context.SubcategoriaAnuncio.AsNoTracking().ToList().Where(m => m.CategoriaAnuncioId == categoriaId).OrderBy(x => x.Nome).ToList();
                return Json(tempSub);
            }
            return Json(false);
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
                    resposta.Close();
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
            ViewData["AgoraTime"] = DateTime.Now;
            return View();
        }
        //Atualizar anuncios
        public async Task<bool> AtualizarAnuncios(String urlOlx)
        {
            Uri uriResult;
            string uriTmp = urlOlx;
            bool result = Uri.TryCreate(uriTmp, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result && Progresso.EtapaAnuncio == 0 && Progresso.EtapaPagina == 0)
            {
                //barra de progresso
                //string[] texto = new string[qntPaginas];

                Progresso.QuantidadePaginas = qntPaginas;
                Progresso.QuantidadeAnuncios = 0;
                Progresso.EtapaPagina = 1;
                Progresso.Estagio = "Páginas";
                Progresso.Gravado = false;


                int posicao = 0;
                int posicaoInicial = 0;
                int tamanho = 0;
                int x = 0;
                int qntAnuncios = 0;
                var anuncios = new List<Anuncio>();
                var anuncioUnico = new Anuncio();
                List<String> listPaginas = new List<string>();

                //criar lista de páginas
                string link = urlOlx; // "https://df.olx.com.br/para-a-sua-casa?sf=1";
                listPaginas.Add(link);
                for (int i = 2; i <= qntPaginas; i++)
                {
                    if (urlOlx.IndexOf("?") > 0)
                    {
                        link = urlOlx.Substring(0, urlOlx.IndexOf("?") + 1) + "o=" + i + "&" + urlOlx.Substring(urlOlx.IndexOf("?") + 1, urlOlx.Length - (urlOlx.IndexOf("?") + 1));
                        listPaginas.Add(link);
                    }
                    else
                    {
                        link = urlOlx + "?o=" + i;
                        listPaginas.Add(link);
                    }
                }

                //baixar páginas
                await getPaginas(listPaginas);

                //BUSCAR ANUNCIOS
                //barra de progresso
                //qntAnuncios = 10; // limitar a quantidade de anuncios
                Progresso.QuantidadeAnuncios = qntAnuncios;
                Progresso.EtapaAnuncio = 1;
                Progresso.Estagio = "Anuncios";
                dadosAnuncios = new List<Anuncio>();

                //listar idanuncios, categorias e subcategorias atuais no bd
                var idAnunciosCadastrados = (from item in _context.Anuncio select new { item.IdAnuncio }).ToList();

                categoriaAnuncios = await _context.CategoriaAnuncio.ToListAsync();
                subcategoriaAnuncios = await _context.SubcategoriaAnuncio.ToListAsync();

                //Total de anuncios
                foreach (String item in conteudoPaginas)
                {
                    x = 0;
                    posicaoInicial = 0;
                    posicao = 0;
                    while (item.IndexOf("data-lurker_list_id", x) > -1)
                    {
                        x = item.IndexOf("data-lurker_list_id", x);
                        x++;
                        qntAnuncios++;

                        posicaoInicial = item.IndexOf("data-lurker_list_id", posicaoInicial) + 10;
                        //IdAnuncio
                        posicao = item.IndexOf("data-lurker_list_id", posicao) + 21;
                        tamanho = item.IndexOf("data-lurker", posicao) - 2 - posicao;
                        if (tamanho > 0)
                        {
                            anuncioUnico.IdAnuncio = item.Substring(posicao, tamanho);
                        }

                        //verifica se o anuncio já está cadastrado
                        //if (_context.Anuncio.FirstOrDefault(o => o.IdAnuncio == anuncioUnico.IdAnuncio) != null)
                        if (idAnunciosCadastrados.FirstOrDefault(o => o.IdAnuncio == anuncioUnico.IdAnuncio) != null)
                        {
                            qntAnuncios--;
                        }
                        else
                        {
                            //Link
                            posicao = item.IndexOf("olx.com.br", posicao) - 11;
                            tamanho = item.IndexOf(" target=", posicao) - 1 - posicao;
                            if (tamanho > 0)
                            {
                                uriTmp = item.Substring(posicao, tamanho);
                                result = Uri.TryCreate(uriTmp, UriKind.Absolute, out uriResult)
                                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                                if (result)
                                {
                                    listaLinksAnuncios.Add(uriTmp);
                                }
                                else
                                {
                                    qntAnuncios--;
                                }
                            }
                        }
                    }
                }

                idAnunciosCadastrados.Clear();

                //BUSCAR ANUNCIOS
                Progresso.QuantidadeAnuncios = qntAnuncios;

                //Busca o último Id
                Anuncio novoAnuncio = await _context.Anuncio.OrderByDescending(o => o.Id).FirstOrDefaultAsync();
                lastId = (novoAnuncio != null) ? novoAnuncio.Id : 0;
                CategoriaAnuncio novoCategoriaAnuncio = await _context.CategoriaAnuncio.OrderByDescending(o => o.Id).FirstOrDefaultAsync();
                lastCategoriaAnuncioId = (novoCategoriaAnuncio != null) ? novoCategoriaAnuncio.Id : 0;
                SubcategoriaAnuncio novoSubcategoriaAnuncio = await _context.SubcategoriaAnuncio.OrderByDescending(o => o.Id).FirstOrDefaultAsync();
                lastSubcategoriaAnuncioId = (novoSubcategoriaAnuncio != null) ? novoSubcategoriaAnuncio.Id : 0;

                //baixar anuncios
                await getAnuncios(listaLinksAnuncios);

                ////Extrair e gravar dados dos anuncios
                //await getDadosAnuncios(conteudoAnuncios);

                //savar anuncios no banco de dados
                _context.Anuncio.AddRange(dadosAnuncios);
                try
                {
                    //salvar categoria e subcategoria
                    if (novasCategoriaAnuncios.Count > 0)
                    {
                        _context.CategoriaAnuncio.AddRange(novasCategoriaAnuncios);
                    }

                    //salvar categoria e subcategoria
                    if (novasSubcategoriaAnuncios.Count > 0)
                    {
                        _context.SubcategoriaAnuncio.AddRange(novasSubcategoriaAnuncios);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException e)
                {
                    Progresso.MensagemErro = e.Message;
                }


                //foreach (Anuncio item in dadosAnuncios)
                //{
                //    _context.Add(item);
                //    _context.SaveChanges();
                //}

                Thread.Sleep(3000);
                Progresso.Gravado = true;
                Progresso.EtapaAnuncio = 0;
                Progresso.EtapaPagina = 0;
                Progresso.MensagemErro = "";
                conteudoPaginas.Clear();
                listaLinksAnuncios.Clear();
                conteudoAnuncios.Clear();
                dadosAnuncios.Clear();
                idAnunciosCadastrados.Clear();
                categoriaAnuncios.Clear();
                subcategoriaAnuncios.Clear();
                lastId = 0;


                //Progresso.Estagio = "";
                return true;
            }
            return true;
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

        //getPaginas multi task
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
            }
        }

        //processUrl das páginas multi task
        private async Task<int> ProcessUrlAsync(string url, WebRequest s_webRequest)
        {
            //atraso para não dar erro 503 de limete de solicitações no site
            Thread.Sleep(50);

            try
            {
                if (Progresso.EtapaPagina < qntPaginas)
                {
                    Progresso.EtapaPagina++;
                }

                s_webRequest = WebRequest.CreateHttp(url);
                s_webRequest.Method = "GET";
                s_webRequest.Proxy = null;

                //s_webRequest.UserAgent = "RequisicaoWebDemo";

                var resposta = await s_webRequest.GetResponseAsync();
                var streamDados = resposta.GetResponseStream();
                var reader = new StreamReader(streamDados);
                var objResponse = reader.ReadToEnd();

                conteudoPaginas.Add(objResponse.ToString());
                reader.Close();
                streamDados.Close();
                resposta.Close();
            }
            catch (WebException e)
            {
                Progresso.MensagemErro = e.Message + " //StackTrace// " + e.StackTrace;
            }
            return 0;
        }

        //getAnuncios multi task
        private async Task getAnuncios(List<string> links)
        {
            Progresso.EtapaAnuncio = 0;
            IEnumerable<Task<int>> downloadTasksQuery =
               from url in links
               select ProcessAnunciosAsync(url, s_WebRequest);

            List<Task<int>> downloadTasks = downloadTasksQuery.ToList();

            while (downloadTasks.Any())
            {
                Task<int> finishedTask = await Task.WhenAny(downloadTasks);
                downloadTasks.Remove(finishedTask);
            }
        }

        //processUrl  multi task
        private async Task<int> ProcessAnunciosAsync(string url, WebRequest s_webRequest)
        {
            try
            {
                Thread.Sleep(5);

                //acessr pagina do anuncio
                if (url.Length < 2083)
                {
                    s_webRequest = WebRequest.CreateHttp(url);
                    s_webRequest.Method = "GET";
                    s_webRequest.Proxy = null;
                    //s_webRequest.UserAgent = "RequisicaoWebDemo";

                    var resposta = await s_webRequest.GetResponseAsync();
                    var streamDados = resposta.GetResponseStream();
                    var reader = new StreamReader(streamDados);
                    var objResponse = reader.ReadToEnd();

                    //conteudoAnuncios.Add(objResponse.ToString());
                    Progresso.EtapaAnuncio++;
                    await ProcessDadosAnunciosAsync(objResponse.ToString());
                    reader.Close();
                    streamDados.Close();
                    resposta.Close();
                }
            }
            catch (Exception e)
            {
                if (e.Message.ToString().IndexOf("error:(404)") > -1)
                {
                    Progresso.MensagemErro = url;
                }
                else
                {
                    Progresso.MensagemErro = url + " / " + e.Message + " / " + e.StackTrace;
                }
                throw;
            }

            return 0;
        }

        private async Task getDadosAnuncios(List<string> anuncios)
        {
            IEnumerable<Task<int>> downloadTasksQuery =
               from anuncio in anuncios
               select ProcessDadosAnunciosAsync(anuncio);

            List<Task<int>> downloadTasks = downloadTasksQuery.ToList();

            while (downloadTasks.Any())
            {
                Task<int> finishedTask = await Task.WhenAny(downloadTasks);
                downloadTasks.Remove(finishedTask);
            }
        }

        //processUrl  multi task
        private async Task<int> ProcessDadosAnunciosAsync(string anuncio)
        {
            try
            {
                var posicao1 = 0;
                var tamanho = 0;
                Anuncio anuncioUnico = new Anuncio();

                //link
                posicao1 = anuncio.IndexOf("canonical") + 17;
                tamanho = anuncio.IndexOf("link ", posicao1) - 4 - posicao1;
                if (tamanho > 0)
                {
                    anuncioUnico.Link = anuncio.Substring(posicao1, tamanho);
                }

                //idAnuncio
                posicao1 = anuncio.IndexOf("adpage/?id=") + 11;
                tamanho = anuncio.IndexOf("meta", posicao1) - 4 - posicao1;
                if (tamanho > 0)
                {
                    anuncioUnico.IdAnuncio = anuncio.Substring(posicao1, tamanho);
                }

                //vendedor
                posicao1 = anuncio.IndexOf("sellerName") + 13;
                tamanho = anuncio.IndexOf(",", posicao1) - 1 - posicao1;
                if (tamanho > 0)
                {
                    anuncioUnico.Vendedor = anuncio.Substring(posicao1, tamanho);
                }

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

                // VER O USO DE INTERSECT PAR COMPARAR TODOS OS ANUNCIOS DE UMA VEZ SÓ

                foreach (string element in excludentes)
                {
                    count++;
                    if (anuncio.IndexOf(element, StringComparison.CurrentCultureIgnoreCase) > -1)
                    {
                        vendedorProibido = true;
                    }
                }
                if (vendedorProibido)
                {
                    //decrementa quantidade na barra de progresso
                    Progresso.QuantidadeAnuncios--;
                    Progresso.EtapaAnuncio--;
                }
                else
                {
                    int categoriaId = 0;
                    //Categoria
                    posicao1 = anuncio.IndexOf("mainCategory\":") + 15;
                    tamanho = anuncio.IndexOf("subCategory\":", posicao1) - 3 - posicao1;

                    if (tamanho > 0)
                    {
                        CategoriaAnuncio tempCategoria = categoriaAnuncios.FirstOrDefault(m => m.Nome == anuncio.Substring(posicao1, tamanho));
                        if (tempCategoria == null)
                        {
                            //cadastrar nova categoria
                            tempCategoria = new CategoriaAnuncio();
                            lastCategoriaAnuncioId++;
                            tempCategoria.Id = lastCategoriaAnuncioId;
                            tempCategoria.Nome = anuncio.Substring(posicao1, tamanho);
                            categoriaAnuncios.Add(tempCategoria);
                            novasCategoriaAnuncios.Add(tempCategoria);
                        }
                        categoriaId = tempCategoria.Id;

                        //SubCategoria
                        posicao1 = anuncio.IndexOf("subCategory\":") + 14;
                        tamanho = anuncio.IndexOf("mainCategoryID", posicao1) - 3 - posicao1;

                        if (tamanho > 0)
                        {
                            //verifica se a subcategoria já existe associada a categoria do anuncio
                            SubcategoriaAnuncio tempSubcategoria = subcategoriaAnuncios.FirstOrDefault(m => m.CategoriaAnuncioId == categoriaId && m.Nome == anuncio.Substring(posicao1, tamanho));
                            if (tempSubcategoria != null)
                            {
                                anuncioUnico.SubcategoriaAnuncioId = tempSubcategoria.Id;
                            }
                            else
                            {
                                //cadastrar nova subcategoria
                                tempSubcategoria = new SubcategoriaAnuncio();
                                lastSubcategoriaAnuncioId++;
                                tempSubcategoria.Id = lastSubcategoriaAnuncioId;
                                tempSubcategoria.Nome = anuncio.Substring(posicao1, tamanho);
                                tempSubcategoria.CategoriaAnuncioId = categoriaId;
                                subcategoriaAnuncios.Add(tempSubcategoria);
                                novasSubcategoriaAnuncios.Add(tempSubcategoria);

                                anuncioUnico.SubcategoriaAnuncioId = tempSubcategoria.Id;
                            }
                        }
                    }
                    if (anuncioUnico.SubcategoriaAnuncioId > 0)
                    {
                        //imagens
                        posicao1 = anuncio.IndexOf("lkx530-4 hXBoAC");
                        count = 0;
                        while (posicao1 > -1 && count < 3)
                        {
                            count++;
                            posicao1 += 27;
                            tamanho = anuncio.IndexOf("alt=", posicao1) - 2 - posicao1;
                            if (tamanho > 0)
                            {
                                Uri uriResult;
                                string uriTmp = anuncio.Substring(posicao1, tamanho);
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
                            posicao1 = anuncio.IndexOf("lkx530-4 hXBoAC", posicao1);
                        }
                        //Descricao
                        posicao1 = anuncio.IndexOf("sc-1sj3nln-1 eOSweo sc-ifAKCX cmFKIN");
                        posicao1 = anuncio.IndexOf("weight", posicao1) + 13;
                        tamanho = anuncio.IndexOf("/span", posicao1) - 1 - posicao1;
                        if (tamanho > 0)
                        {
                            anuncioUnico.Descricao = anuncio.Substring(posicao1, tamanho);
                        }
                        //Titulo
                        posicao1 = anuncio.IndexOf("og:title");
                        if (posicao1 > 0)
                        {
                            posicao1 += 19;
                            tamanho = anuncio.IndexOf("/><meta", posicao1) - 1 - posicao1;

                            if (tamanho > 0)
                            {
                                anuncioUnico.Titulo = anuncio.Substring(posicao1, tamanho);
                            }
                        }
                        //Data Publicação
                        posicao1 = anuncio.IndexOf("Publicado em <") + 21;
                        tamanho = 5;
                        anuncioUnico.DtPublicacao = DateTime.ParseExact(anuncio.Substring(posicao1, tamanho), "dd/MM", CultureInfo.InvariantCulture);
                        //preço
                        posicao1 = anuncio.IndexOf("price") + 8;
                        tamanho = anuncio.IndexOf(",", posicao1) - 1 - posicao1;
                        if (tamanho > 0)
                        {
                            anuncioUnico.VlAnunciado = Int32.Parse(anuncio.Substring(posicao1, tamanho));
                        }
                        //bairro
                        posicao1 = anuncio.IndexOf("Bairro<");
                        if (posicao1 > 0)
                        {
                            posicao1 = anuncio.IndexOf("<dd ", posicao1);
                            posicao1 = anuncio.IndexOf(">", posicao1) + 1;
                            tamanho = anuncio.IndexOf("</dd>", posicao1) - posicao1;
                            if (tamanho > 0)
                            {
                                anuncioUnico.Bairro = anuncio.Substring(posicao1, tamanho);
                            }
                        }
                        else
                        {
                            posicao1 = anuncio.IndexOf("Município<");
                            if (posicao1 > 0)
                            {
                                posicao1 = anuncio.IndexOf("<dd ", posicao1);
                                posicao1 = anuncio.IndexOf(">", posicao1) + 1;
                                tamanho = anuncio.IndexOf("</dd>", posicao1) - posicao1;

                                if (tamanho > 0)
                                {
                                    anuncioUnico.Bairro = anuncio.Substring(posicao1, tamanho);
                                }
                            }
                        }
                        //telefone
                        posicao1 = anuncio.IndexOf(";phone") + 38;
                        tamanho = anuncio.IndexOf("&quot", posicao1) - posicao1;

                        if (tamanho > 0)
                        {
                            anuncioUnico.Telefone = anuncio.Substring(posicao1, tamanho);
                        }
                        // inclusão dos dados do anuncio na lista dadosAnuncios
                        anuncioUnico.Id = (lastId > 0) ? lastId + 1 : 1;
                        lastId++;
                        dadosAnuncios.Add(anuncioUnico);

                        //OlxPay
                        posicao1 = anuncio.IndexOf("olxPay\":{\"enabled");
                        if (posicao1 > 0)
                        {
                            anuncioUnico.OlxPay = true;
                        }
                        else
                        {
                            anuncioUnico.OlxPay = false;
                        }
                        //OlxDelivery
                        posicao1 = anuncio.IndexOf("olxDelivery\":{\"enabled");
                        if (posicao1 > 0)
                        {
                            anuncioUnico.OlxDelivery = true;
                        }
                        else
                        {
                            anuncioUnico.OlxDelivery = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Progresso.MensagemErro = e.Message + " / " + e.StackTrace;
                throw;
            }

            return 0;
        }
    }
}
