using ControleDeLeiloes.Data;
using ControleDeLeiloes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace ControleDeLeiloes.Controllers
{
    public class LeiloesController : Controller
    {
        private readonly ControleDeLeiloesDbContext _context;
        public LeiloesController(ControleDeLeiloesDbContext context){_context = context;}
        public async Task<IActionResult> Index()
        {
            var leilao = _context.Leilao.Include(m => m.Leiloeiro).ToListAsync();
            return View(await leilao);
        }
        public IActionResult Create()
        {
            ViewData["leiloeiros"] = new SelectList(_context.Leiloeiro, "Id", "Nome");
            return View();
        }
        // POST: Leiloes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Leilao leilao)
        {
            if (ModelState.IsValid)
            {
                _context.Add(leilao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(leilao);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) { return NotFound(); }
            var leilao = await _context.Leilao
                .Include(m => m.Leiloeiro)
                .Include(m => m.Lotes)
                .ThenInclude(m => m.LoteProdutos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leilao == null)
            {
                return NotFound();
            }
            return View(leilao);
        }
        // GET: Leiloes/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["leiloeiros"] = new SelectList(_context.Leiloeiro, "Id", "Nome");
            var leilao = await _context.Leilao
                .Include(m=> m.Leiloeiro)
                .FirstOrDefaultAsync(m=> m.Id == id);
            if (leilao == null)
            {
                return NotFound();
            }
            return View(leilao);
        }

        // POST: Leiloes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Leilao leilao)
        {
            if (id != leilao.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leilao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeilaoExists(leilao.Id))
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
            return View(leilao);
        }

        // GET: Leiloes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leilao = await _context.Leilao
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leilao == null)
            {
                return NotFound();
            }

            return View(leilao);
        }

        // POST: Leiloes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leilao = await _context.Leilao.FindAsync(id);
            _context.Leilao.Remove(leilao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LeilaoExists(int id)
        {
            return _context.Leilao.Any(e => e.Id == id);
        }
    }
}
