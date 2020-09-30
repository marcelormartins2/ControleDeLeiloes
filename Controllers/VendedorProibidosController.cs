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
    public class VendedorProibidosController : Controller
    {
        private readonly ControleDeLeiloesDbContext _context;

        public VendedorProibidosController(ControleDeLeiloesDbContext context)
        {
            _context = context;
        }

        // GET: VendedorProibidos
        public async Task<IActionResult> Index()
        {
            return View(await _context.VendedorProibido.ToListAsync());
        }

        // GET: VendedorProibidos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendedorProibido = await _context.VendedorProibido
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vendedorProibido == null)
            {
                return NotFound();
            }

            return View(vendedorProibido);
        }

        // GET: VendedorProibidos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VendedorProibidos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdVendedor,Nome")] VendedorProibido vendedorProibido)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vendedorProibido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vendedorProibido);
        }

        // GET: VendedorProibidos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendedorProibido = await _context.VendedorProibido.FindAsync(id);
            if (vendedorProibido == null)
            {
                return NotFound();
            }
            return View(vendedorProibido);
        }

        // POST: VendedorProibidos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdVendedor,Nome")] VendedorProibido vendedorProibido)
        {
            if (id != vendedorProibido.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vendedorProibido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendedorProibidoExists(vendedorProibido.Id))
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
            return View(vendedorProibido);
        }

        // GET: VendedorProibidos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendedorProibido = await _context.VendedorProibido
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vendedorProibido == null)
            {
                return NotFound();
            }

            return View(vendedorProibido);
        }

        // POST: VendedorProibidos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vendedorProibido = await _context.VendedorProibido.FindAsync(id);
            _context.VendedorProibido.Remove(vendedorProibido);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VendedorProibidoExists(int id)
        {
            return _context.VendedorProibido.Any(e => e.Id == id);
        }
    }
}
