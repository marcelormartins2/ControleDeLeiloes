using ControleDeLeiloes.Data;
using ControleDeLeiloes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ControleDeLeiloes.Controllers
{
    public class LotesController : Controller
    {
        private readonly ControleDeLeiloesDbContext _context;

        public LotesController(ControleDeLeiloesDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var lote = await _context.Lote
                .Include(a => a.Leilao)
                .Include(b => b.Produto)
                .ToListAsync();
            return View(lote);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lote = await _context.Lote
                .ToListAsync();
            var Lote = await _context.Lote
                .Include(a => a.Leilao)
                .Include(b => b.Produto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Lote == null)
            {
                return NotFound();
            }

            return View(Lote);
        }

        public IActionResult Create()
        {
            ViewData["produtos"] = new SelectList(_context.Produto, "Id", "Titulo");
            ViewData["leiloes"] = new SelectList(_context.Leilao, "Id", "Descricao");
            return View();
        }

        // POST: Lotes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lote Lote)
        {
            if (ModelState.IsValid)
            {
                _context.Add(Lote);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Leiloes", new { Id = Lote.LeilaoId });
            }
            return View(Lote);
        }

        // GET: Lotes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["produtos"] = new SelectList(_context.Produto, "Id", "Titulo");
            ViewData["leiloes"] = new SelectList(_context.Leilao, "Id", "Descricao");

            var Lote = await _context.Lote.FindAsync(id);
            if (Lote == null)
            {
                return NotFound();
            }
            return View(Lote);
        }

        // POST: Lotes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numero,VlAvalicao,VlCondicional,VlPago,VlLance,ProdutoId,LeilaoId")] Lote lote)
        {
            if (lote == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lote);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoteExists(lote.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Leiloes", new { Id = lote.LeilaoId });
            }
            return View(lote);
        }

        // GET: Lotes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Lote = await _context.Lote
                .Include(l => l.Leilao)
                .Include(p => p.Produto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (Lote == null)
            {
                return NotFound();
            }

            return View(Lote);
        }

        // POST: Lotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lote = await _context.Lote.FindAsync(id);
            _context.Lote.Remove(lote);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Leiloes", new { Id = lote.LeilaoId });
        }

        private bool LoteExists(int? id)
        {
            return _context.Lote.Any(e => e.Id == id);
        }
        public bool AlterarLance(int id, int val)
        {
            return true;
        }

    }
}
