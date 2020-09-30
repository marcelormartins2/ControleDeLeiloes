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
                using (resposta = await requisicaoWeb.GetResponseAsync())
                {
                    streamDados = resposta.GetResponseStream();
                    reader = new StreamReader(streamDados);
                    objResponse = reader.ReadToEnd();
                    texto = objResponse.ToString();
                }
                //IdVendedor
                posicao = texto.IndexOf("data-lurker_list_position", posicao) + 33;
                tamanho = texto.IndexOf(" target=", posicao) - 1 - posicao;
                if (tamanho > 0)
                {
                    anuncioUnico.Link = texto.Substring(posicao, tamanho);
                }

                //Verifica se o vendedor é proíbido



            }
            produtos.Add(produtoUnico);
            }



            //Obter descrição
            posicao = texto.IndexOf("og:title");
                        if (posicao > 0)
                        {
                    posicao += 19;
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
               
            jsonString = JsonSerializer.Serialize(produto);
            return Json(jsonString);


            if (ModelState.IsValid)
            {
                _context.Add(anuncio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdAnuncio,Link,DtPublicacao,Img1,Img2,Img3,Descricao,VlAnunciado,Bairro,Telefone,Vendedor,CodVendedor,DtVendedorDesde")] Anuncio anuncio)
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
