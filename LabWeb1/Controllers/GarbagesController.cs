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
    public class GarbagesController : Controller
    {
        private readonly RecyclingNowContext _context;

        public GarbagesController(RecyclingNowContext context)
        {
            _context = context;
        }

        // GET: Garbages
        public async Task<IActionResult> Index()
        {
            return View(await _context.Garbage.ToListAsync());
        }


        // GET: Garbages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garbage = await _context.Garbage
                .FirstOrDefaultAsync(m => m.Id == id);
            if (garbage == null)
            {
                return NotFound();
            }

            return View(garbage);
        }
        ///aaaaaaaaaaaaaa
        public async Task<IActionResult> Materials(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garbage = await _context.Garbage
                .FirstOrDefaultAsync(m => m.Id == id);
            if (garbage == null)
            {
                return NotFound();
            }
            ViewBag.IdGarbage = id;

            return RedirectToAction("Index", "GarbageMaterials", new { id = garbage.Id });
        }

        // GET: Garbages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Garbages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Garbage garbage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(garbage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(garbage);
        }

        // GET: Garbages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garbage = await _context.Garbage.FindAsync(id);
            if (garbage == null)
            {
                return NotFound();
            }
            return View(garbage);
        }

        // POST: Garbages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Garbage garbage)
        {
            if (id != garbage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(garbage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GarbageExists(garbage.Id))
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
            return View(garbage);
        }

        // GET: Garbages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garbage = await _context.Garbage
                .FirstOrDefaultAsync(m => m.Id == id);
            if (garbage == null)
            {
                return NotFound();
            }

            return View(garbage);
        }

        // POST: Garbages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var garbage = await _context.Garbage.FindAsync(id);
            DeleteGarbageInMaterial(id);
            _context.Garbage.Remove(garbage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GarbageExists(int id)
        {
            return _context.Garbage.Any(e => e.Id == id);
        }
        private async void DeleteGarbageInMaterial(int id)
        {
            var garbage = await _context.Garbage.FindAsync(id);

            foreach (var i in _context.GarbageMaterial)
            {
                if (i.IdGarbage == garbage.Id)
                    _context.GarbageMaterial.Remove(i);
            }
        }
    }
}
