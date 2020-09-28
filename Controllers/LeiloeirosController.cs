using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleDeLeiloes.Data;
using ControleDeLeiloes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControleDeLeiloes.Controllers
{
    public class LeiloeirosController : Controller
    {
        private readonly ControleDeLeiloesDbContext _context;
        //private readonly UserManager<Parceiro> _userManager;
        //private readonly SignInManager<Parceiro> _signInManager;
        //private readonly RoleManager<IdentityRole> _roleManager;

        public LeiloeirosController(ControleDeLeiloesDbContext context
        //UserManager<Parceiro> userManager,
        //SignInManager<Parceiro> signInManager
        //RoleManager<IdentityRole> roleManager,
        )
        {
            _context = context;
            //_userManager = userManager;
            //_signInManager = signInManager;
            //_roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Leiloeiro.ToListAsync());
        }

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

        public IActionResult Create()
        {
            return View();
        }

        // POST: Leiloeiros/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Leiloeiro leiloeiro)
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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Leiloeiro leiloeiro)
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
