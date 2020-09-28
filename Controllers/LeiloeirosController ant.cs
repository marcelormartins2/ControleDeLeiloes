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
    public class LeiloeirosControllerant : Controller
    {
        private readonly ControleDeLeiloesDbContext _context;

        public LeiloeirosControllerant(ControleDeLeiloesDbContext context)
        {
            _context = context;
        }

        // GET: Leiloeiros
        public async Task<IActionResult> Index()
        {
            return View(await _context.Leiloeiro.ToListAsync());
        }

        // GET: Leiloeiros/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leiloeiro = await _context.Leiloeiro
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leiloeiro == null)
            {
                return NotFound();
            }

            return View(leiloeiro);
        }

        // GET: Leiloeiros/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Leiloeiros/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Telefone,Site,TaxaAvaliacaoPadrao,TaxaVendaPadrao")] Leiloeiro leiloeiro)
        {
            if (ModelState.IsValid)
            {
                _context.Add(leiloeiro);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(leiloeiro);
        }

        // GET: Leiloeiros/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leiloeiro = await _context.Leiloeiro.FindAsync(id);
            if (leiloeiro == null)
            {
                return NotFound();
            }
            return View(leiloeiro);
        }

        // POST: Leiloeiros/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Telefone,Site,TaxaAvaliacaoPadrao,TaxaVendaPadrao")] Leiloeiro leiloeiro)
        {
            if (id != leiloeiro.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leiloeiro);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeiloeiroExists(leiloeiro.Id))
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
            return View(leiloeiro);
        }

        // GET: Leiloeiros/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leiloeiro = await _context.Leiloeiro
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leiloeiro == null)
            {
                return NotFound();
            }

            return View(leiloeiro);
        }

        // POST: Leiloeiros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leiloeiro = await _context.Leiloeiro.FindAsync(id);
            _context.Leiloeiro.Remove(leiloeiro);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LeiloeiroExists(int id)
        {
            return _context.Leiloeiro.Any(e => e.Id == id);
        }
    }
}
