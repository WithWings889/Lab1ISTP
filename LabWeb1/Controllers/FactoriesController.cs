using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabWeb1;

namespace LabWeb1.Controllers
{
    public class FactoriesController : Controller
    {
        private readonly RecyclingNowContext _context;

        public FactoriesController(RecyclingNowContext context)
        {
            _context = context;
        }

        // GET: Factories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Factory.ToListAsync());
        }

        // GET: Factories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factory = await _context.Factory
                .FirstOrDefaultAsync(m => m.Id == id);
            if (factory == null)
            {
                return NotFound();
            }
            return View(factory);
        }
        public async Task<IActionResult> GarbageTypes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factory = await _context.Factory
                .FirstOrDefaultAsync(m => m.Id == id);
            if (factory == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "FactoryGarbageTypes", new { id = factory.Id });
        }

        // GET: Factories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Factories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Website")] Factory factory)
        {
            if (ModelState.IsValid)
            {
                var f = _context.Factory.Where(c => c.Name == factory.Name && c.Address == factory.Address).FirstOrDefault();
                if (f != null)
                {
                    ModelState.AddModelError(string.Empty, "Фабрика з такою назвою та за такою ж адресою вже існує");
                }
                _context.Add(factory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(factory);
        }

        // GET: Factories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factory = await _context.Factory.FindAsync(id);
            if (factory == null)
            {
                return NotFound();
            }
            return View(factory);
        }

        // POST: Factories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Website")] Factory factory)
        {
            if (id != factory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(factory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FactoryExists(factory.Id))
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
            return View(factory);
        }

        // GET: Factories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factory = await _context.Factory
                .FirstOrDefaultAsync(m => m.Id == id);
            if (factory == null)
            {
                return NotFound();
            }

            return View(factory);
        }

        // POST: Factories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var factory = await _context.Factory.FindAsync(id);
            DeleteFactoryInGarbageType(id);
            _context.Factory.Remove(factory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FactoryExists(int id)
        {
            return _context.Factory.Any(e => e.Id == id);
        }
        private async void DeleteFactoryInGarbageType(int id)
        {
            var factory = await _context.Factory.FindAsync(id);

            foreach (var i in _context.FactoryGarbageType)
            {
                if (i.IdFactory == factory.Id)
                    _context.FactoryGarbageType.Remove(i);
            }
        }
    }
}
