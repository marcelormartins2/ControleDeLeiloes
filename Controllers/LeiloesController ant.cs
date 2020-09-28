using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControleDeLeiloes.Data;
using ControleDeLeiloes.Models;

namespace ControleDeLeiloes.Controllers
{
    public class LeiloesControllerant : Controller
    {
        private readonly ControleDeLeiloesDbContext _context;

        public LeiloesControllerant(ControleDeLeiloesDbContext context)
        {
            _context = context;
        }

        // GET: Leilaos
        public async Task<IActionResult> Index()
        {
            var controleDeLeiloesDbContext = _context.Leilao.Include(l => l.Leiloeiro);
            return View(await controleDeLeiloesDbContext.ToListAsync());
        }

        // GET: Leilaos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leilao = await _context.Leilao
                .Include(l => l.Leiloeiro)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leilao == null)
            {
                return NotFound();
            }

            return View(leilao);
        }

        // GET: Leilaos/Create
        public IActionResult Create()
        {
            ViewData["LeiloeiroId"] = new SelectList(_context.Leiloeiro, "Id", "Id");
            return View();
        }

        // POST: Leilaos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descricao,Data,TaxaAvaliacao,TaxaVenda,LeiloeiroId")] Leilao leilao)
        {
            if (ModelState.IsValid)
            {
                _context.Add(leilao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LeiloeiroId"] = new SelectList(_context.Leiloeiro, "Id", "Id", leilao.LeiloeiroId);
            return View(leilao);
        }

        // GET: Leilaos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leilao = await _context.Leilao.FindAsync(id);
            if (leilao == null)
            {
                return NotFound();
            }
            ViewData["LeiloeiroId"] = new SelectList(_context.Leiloeiro, "Id", "Id", leilao.LeiloeiroId);
            return View(leilao);
        }

        // POST: Leilaos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descricao,Data,TaxaAvaliacao,TaxaVenda,LeiloeiroId")] Leilao leilao)
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
            ViewData["LeiloeiroId"] = new SelectList(_context.Leiloeiro, "Id", "Id", leilao.LeiloeiroId);
            return View(leilao);
        }

        // GET: Leilaos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leilao = await _context.Leilao
                .Include(l => l.Leiloeiro)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leilao == null)
            {
                return NotFound();
            }

            return View(leilao);
        }

        // POST: Leilaos/Delete/5
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
