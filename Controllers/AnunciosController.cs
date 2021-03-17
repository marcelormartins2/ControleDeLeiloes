using ControleDeLeiloes.Data;
using ControleDeLeiloes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;

namespace ControleDeLeiloes.Controllers
{
    public class AnunciosController : Controller
    {
        private readonly ControleDeLeiloesDbContext _context;

        public AnunciosController(ControleDeLeiloesDbContext context)
        {
            _context = context;
        }

        private static readonly HttpClient clienteHttp = new HttpClient();
        static List<String> conteudoPaginas = new List<string>();
        static List<String> listaLinksAnuncios = new List<string>();
        static List<String> conteudoAnuncios = new List<string>();
        static List<Anuncio> dadosAnuncios = new List<Anuncio>();
        static List<CategoriaAnuncio> categoriaAnuncios = new List<CategoriaAnuncio>();
        static List<SubcategoriaAnuncio> subcategoriaAnuncios = new List<SubcategoriaAnuncio>();
        static List<CategoriaAnuncio> novasCategoriaAnuncios = new List<CategoriaAnuncio>();
        static List<SubcategoriaAnuncio> novasSubcategoriaAnuncios = new List<SubcategoriaAnuncio>();
        static List<UrlAnuncio> UrlAnuncios = new List<UrlAnuncio>();
        static int qntPaginas = 5;
        static int lastId = 0;
        static int lastCategoriaAnuncioId = 0;
        static int lastSubcategoriaAnuncioId = 0;

        //ProgressBar
        private static Progresso Progresso = new Progresso { RegistrosBD = -1 };
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
        public async Task<IActionResult> Index(int? pagina, bool verNotView, int? categoriaId, int? subcategoriaId, string uf, string bairro, bool olxPay, bool olxDelivery, int? vlrMin, int? vlrMax, string txtBusca)
        {
            const int itensPorPagina = 100;
            int numeroPagina = (pagina ?? 1); //se pagina for null
            int tempCategoriaId = _context.CategoriaAnuncio.OrderBy(m => m.Nome).FirstOrDefault().Id;
            olxPay = olxDelivery == true ? true : olxPay;
            ViewData["verNotView"] = verNotView;
            ViewBag.olxPay = olxPay;
            ViewBag.olxDelivery = olxDelivery;
            ViewBag.uf = new SelectList(_context.Anuncio.Select(m => m.UF).Distinct()).OrderBy(m => m);
            ViewData["ufAtual"] = uf;
            ViewBag.bairro = new SelectList(_context.Anuncio.Select(m => m.Bairro).Distinct()).OrderBy(m => m);
            ViewData["bairroAtual"] = bairro;
            if (categoriaId > 0)
            {
                ViewBag.categoriaId = new SelectList(_context.CategoriaAnuncio.OrderBy(m => m.Nome), "Id", "Nome", categoriaId);
                if (subcategoriaId > 0)
                {
                    ViewBag.subcategoriaId = new SelectList(_context.SubcategoriaAnuncio.Where(m => m.CategoriaAnuncioId == categoriaId).OrderBy(m => m.Nome), "Id", "Nome", subcategoriaId);
                }
                else
                {
                    ViewBag.subcategoriaId = new SelectList(_context.SubcategoriaAnuncio.Where(m => m.CategoriaAnuncioId == categoriaId).OrderBy(m => m.Nome), "Id", "Nome");
                }
            }
            else
            {
                ViewBag.categoriaId = new SelectList(_context.CategoriaAnuncio.OrderBy(m => m.Nome), "Id", "Nome");
                ViewBag.subcategoriaId = new SelectList(_context.SubcategoriaAnuncio.Where(m => m.CategoriaAnuncioId == tempCategoriaId).OrderBy(m => m.Nome), "Id", "Nome");
            }


            //cria consulta
            var tempAnuncio = _context.Anuncio.Include(m => m.SubcategoriaAnuncio).Where(
                    m => m.OlxPay == olxPay
                    && m.OlxDelivery == olxDelivery
                    && m.NotView == verNotView).AsNoTracking();
            if (uf != "" && uf != null)
            {
                tempAnuncio = tempAnuncio.Where(m => m.UF == uf);
            }
            if (bairro != "" && bairro != null)
            {
                tempAnuncio = tempAnuncio.Where(m => m.Bairro == bairro);
            }
            if ((categoriaId == null || categoriaId == 0) && (subcategoriaId == null || subcategoriaId == 0))
            {
                tempAnuncio = tempAnuncio.Where(m => m.SubcategoriaAnuncio.CategoriaAnuncioId == tempCategoriaId);
                ViewData["subCategoriaEmpty"] = true;
            }
            else if (categoriaId != null && categoriaId != 0)
            {
                if (subcategoriaId != null && subcategoriaId != 0)
                {
                    tempAnuncio = tempAnuncio.Where(m => m.SubcategoriaAnuncioId == subcategoriaId);
                    ViewData["subCategoriaEmpty"] = false;
                }
                else
                {
                    tempAnuncio = tempAnuncio.Where(m => m.SubcategoriaAnuncio.CategoriaAnuncioId == categoriaId);
                    ViewData["subCategoriaEmpty"] = true;
                }
            }
            else
            {
                tempAnuncio = tempAnuncio.Where(m => m.SubcategoriaAnuncioId == subcategoriaId);
                ViewData["subCategoriaEmpty"] = false;
            }
            if (vlrMin > 0)
            {
                tempAnuncio = tempAnuncio.Where(m => m.VlAnunciado > vlrMin);
            }
            if (vlrMax > 0)
            {
                tempAnuncio = tempAnuncio.Where(m => m.VlAnunciado < vlrMax);
            }
            if (txtBusca != "" && txtBusca != null)
            {
                tempAnuncio = tempAnuncio.Where(m => m.Titulo.Contains(txtBusca));
            }
            return View(await tempAnuncio.ToPagedListAsync(numeroPagina, itensPorPagina));
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
            if (Progresso.RegistrosBD < 1)
            {
                ServicePointManager.DefaultConnectionLimit = 10;
                List<string> listAnuncios = await _context.Anuncio.OrderBy(c => c.DtPublicacao).Select(c => c.Link).ToListAsync();
                Progresso.RegistrosBD = listAnuncios.Count();
                Progresso.RegistrosBDApagados = 0;
                Progresso.RegistrosBDAnalisados = 0;

                IEnumerable<Task<int>> downloadTasksQuery =
                   from url in listAnuncios
                   select ProcessDeletarInvalidos(url);


                List<Task<int>> downloadTasks = downloadTasksQuery.ToList();

                while (downloadTasks.Any())
                {
                    Task<int> finishedTask = await Task.WhenAny(downloadTasks);
                    downloadTasks.Remove(finishedTask);
                }
                Progresso.RegistrosBD = 0;
            }
            return true;
        }
        private async Task<int> ProcessDeletarInvalidos(string url)
        {
            //atraso para não dar erro 503 de limete de solicitações no site
            Thread.Sleep(50);

            //WebResponse resposta = null;
            try
            {
                var resposta = await clienteHttp.GetAsync(url);
                Progresso.RegistrosBDAnalisados++;
                if (resposta.StatusCode == HttpStatusCode.NotFound)
                {
                    Progresso.RegistrosBDApagados++;
                    var anuncioDeletar = await _context.Anuncio.Where(c => c.Link == url).FirstOrDefaultAsync();
                    _context.Anuncio.Remove(anuncioDeletar);
                    await _context.SaveChangesAsync();
                }

                //WebRequest requisicaoWeb = WebRequest.Create(url);

                //requisicaoWeb.Method = "HEAD";
                ////requisicaoWeb.UserAgent = "RequisicaoWebDemo";

                //resposta = await requisicaoWeb.GetResponseAsync();
                //resposta.Close();
            }
            catch (WebException e)
            {
                Progresso.MensagemErro = e.Message.ToString();
            }

            return 0;
        }
        //Atualizar anuncios
        public async Task<bool> AtualizarAnuncios(String urlOlx, int numPaginas)
        {
            Uri uriResult;
            string uriTmp = urlOlx;
            bool result = Uri.TryCreate(uriTmp, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result && Progresso.EtapaAnuncio == 0 && Progresso.EtapaPagina == 0 && numPaginas > 0)
            {
                //barra de progresso
                //string[] texto = new string[qntPaginas];
                qntPaginas = numPaginas;
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

                String agrupaPaginas = string.Join(",", conteudoPaginas.ToArray());

                string pattern = @"\bdata-lurker_list_id\b";
                Regex rgx = new Regex(pattern);

                qntAnuncios = rgx.Matches(agrupaPaginas).Count();

                qntAnuncios = 0;

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

                ////Extrair dados dos anuncios
                await getDadosAnuncios(conteudoAnuncios);

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
                Progresso.EtapaDados = 0;
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
        private async Task getPaginas(List<string> listPaginas)
        {
            ServicePointManager.DefaultConnectionLimit = 10;

            IEnumerable<Task<int>> downloadTasksQuery =
               from url in listPaginas
               select ProcessUrlAsync(url);


            List<Task<int>> downloadTasks = downloadTasksQuery.ToList();

            while (downloadTasks.Any())
            {
                Task<int> finishedTask = await Task.WhenAny(downloadTasks);
                downloadTasks.Remove(finishedTask);
            }
        }
        //processUrl das páginas multi task
        private async Task<int> ProcessUrlAsync(string url)
        {
            //atraso para não dar erro 503 de limete de solicitações no site
            Thread.Sleep(50);

            try
            {
                var resposta = await clienteHttp.GetStringAsync(url);
                conteudoPaginas.Add(resposta);
                if (Progresso.EtapaPagina < qntPaginas)
                {
                    Progresso.EtapaPagina++;
                }
            }
            catch (WebException e)
            {
                Progresso.MensagemErro = "Erro ProcessUrlAsync";
                //Progresso.MensagemErro = e.Message + " //StackTrace// " + e.StackTrace;
            }
            return 0;
        }
        //getAnuncios multi task
        private async Task getAnuncios(List<string> links)
        {
            Progresso.EtapaAnuncio = 0;

            IEnumerable<Task> downloadTasksQuery =
               from url in links
               select ProcessAnunciosAsync(url);

            List<Task> downloadTasks = downloadTasksQuery.ToList();

            while (downloadTasks.Any())
            {
                Task finishedTask = await Task.WhenAny(downloadTasks);
                Progresso.MensagemErro = conteudoAnuncios.Count().ToString();
                downloadTasks.Remove(finishedTask);
            }
            //downloadTasks.Clear();
        }
        //processUrl  multi task
        private async Task ProcessAnunciosAsync(string url)
        {
            try
            {
                Thread.Sleep(5);

                //acessr pagina do anuncio
                if (url.Length < 2083)
                {
                    var resposta = await clienteHttp.GetStringAsync(url);
                    Progresso.EtapaAnuncio++;
                    conteudoAnuncios.Add(resposta);
                }
            }
            catch (WebException webex)
            {
                Progresso.MensagemErro = "Erro ProcessAnunciosAsync - webexception";
                //Progresso.MensagemErro = url + " / " + webex.Message + " / " + webex.StackTrace;
                //WebResponse errResp = webex.Response;
                //using (Stream respStream = errResp.GetResponseStream())
                //{
                //    StreamReader reader = new StreamReader(respStream);
                //    Progresso.MensagemErro = reader.ReadToEnd();
                //}
            }
            catch (Exception e)
            {
                Progresso.MensagemErro = "Erro ProcessAnunciosAsync";
                //if (e.Message.ToString().IndexOf("error:(404)") > -1)
                //{
                //    Progresso.MensagemErro = url;
                //}
                //else
                //{
                //    Progresso.MensagemErro = url + " / " + e.Message + " / " + e.StackTrace;
                //}
                //throw;
            }
        }
        private async Task getDadosAnuncios(List<string> anuncios)
        {
            Progresso.EtapaDados = 0;
            Progresso.QuantidadeAnuncios = anuncios.Count();
            Progresso.EtapaAnuncio = anuncios.Count();
            IEnumerable<Task> downloadTasksQuery =
               from anuncio in anuncios
               select Task.Run(() => ProcessDadosAnunciosAsync(anuncio));

            List<Task> downloadTasks = downloadTasksQuery.ToList();

            while (downloadTasks.Any())
            {
                Task finishedTask = await Task.WhenAny(downloadTasks);
                downloadTasks.Remove(finishedTask);
            }

            //Task t = Task.WhenAll(downloadTasksQuery);
            //try
            //{
            //    t.Wait();
            //}
            //catch { }

            //if (t.Status == TaskStatus.RanToCompletion)
            //    Progresso.MensagemErro = "Todas os dados processados com sucesso";
            //else if (t.Status == TaskStatus.Faulted)
            //    Progresso.MensagemErro = "Falha em alguns dados";

        }
        //processUrl  multi task
        private Task ProcessDadosAnunciosAsync(string anuncio)
        {
            try
            {
                var posicao1 = 0;
                var tamanho = 0;
                Anuncio anuncioUnico = new Anuncio();
                Progresso.EtapaDados++;

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
                    Progresso.EtapaDados--;
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
                        posicao1 = anuncio.IndexOf(";listTime") + 22;
                        tamanho = 10;
                        anuncioUnico.DtPublicacao = DateTime.ParseExact(anuncio.Substring(posicao1, tamanho), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        //preço
                        posicao1 = anuncio.IndexOf("price") + 8;
                        tamanho = anuncio.IndexOf(",", posicao1) - 1 - posicao1;
                        if (tamanho > 0)
                        {
                            anuncioUnico.VlAnunciado = Int32.Parse(anuncio.Substring(posicao1, tamanho));
                        }
                        //UF
                        if (anuncioUnico.Link.Length > 0)
                        {
                            anuncioUnico.UF = anuncioUnico.Link.Substring(8, 2).ToUpper();
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
                        posicao1 = anuncio.IndexOf("olxPay\":{\"enabled\":true");
                        if (posicao1 > 0)
                        {
                            anuncioUnico.OlxPay = true;
                        }
                        else
                        {
                            anuncioUnico.OlxPay = false;
                        }
                        //OlxDelivery
                        posicao1 = anuncio.IndexOf("olxDelivery\":{\"enabled\":true");
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
                Progresso.MensagemErro = "Erro ProcessamentoDadosDoAnuncio"; //e.Message + e.StackTrace;
            }

            return null;
        }
        // GET: Anuncios/Create
        public IActionResult Create()
        {
            ViewData["AgoraTime"] = DateTime.Now;
            return View();
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
    }
}
