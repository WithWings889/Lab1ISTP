using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1;

namespace WebApplication1.Controllers
{
    public class GarbageMaterialsController : Controller
    {
        private readonly RecyclingNowContext _context;

        public GarbageMaterialsController(RecyclingNowContext context)
        {
            _context = context;
        }

        // GET: GarbageMaterials
        public async Task<IActionResult> Index(int? id)
        {
            ViewBag.IdGarbage = id;
            ViewBag.GarbageName = _context.Garbage.Find(id).Name;
            var recyclingNowContext = _context.GarbageMaterial.Where(e => e.IdGarbage == id).Include(g => g.IdGarbageNavigation).Include(g => g.IdMaterialNavigation);
            return View(await recyclingNowContext.ToListAsync());
        }

        // GET: GarbageMaterials/Details/5
        public async Task<IActionResult> Details(int? id, int? IdGarbage)
        {
            ViewBag.IdGarbage = IdGarbage;
            if (id == null)
            {
                return NotFound();
            }
            var IdMaterial = _context.GarbageMaterial.Find(id).IdMaterial;
            var garbageMaterial = await _context.GarbageMaterial
                .Include(g => g.IdGarbageNavigation)
                .Include(g => g.IdMaterialNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (garbageMaterial == null)
            {
                return NotFound();
            }
            ViewBag.GarbageName = garbageMaterial.IdGarbageNavigation.Name;
            ViewBag.Count = _context.GarbageMaterial.Where(b => b.IdMaterial == IdMaterial).Include(b => b.IdGarbageNavigation).Count();
            ViewBag.GarbagesList = _context.GarbageMaterial.Where(b => b.IdMaterial == IdMaterial).Include(b => b.IdGarbageNavigation).ToList();
            return View(garbageMaterial);
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

        // GET: GarbageMaterials/Create
        public IActionResult Create(int? idGarbage)
        {

            ViewBag.IdGarbage = idGarbage;
            ViewBag.GarbageName = _context.Garbage.Where(g => g.Id == idGarbage).FirstOrDefault().Name;
            ViewData["IdGarbage"] = new SelectList(_context.Garbage.Where(b => b.Id == idGarbage), "Id", "Name");
            ViewData["IdMaterial"] = new SelectList(_context.Material, "Id", "Name");
            return View();
        }

        // POST: GarbageMaterials/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int idGarbage, [Bind("Id,IdGarbage,IdMaterial")] GarbageMaterial garbageMaterial)
        {
            garbageMaterial.IdGarbage = idGarbage;
            if (ModelState.IsValid)
            {
                _context.Add(garbageMaterial);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "GarbageMaterials",
                    new { id = idGarbage, name = _context.Garbage.Where(c => c.Id == idGarbage).FirstOrDefault().Name });
            }
            ViewData["IdGarbage"] = new SelectList(_context.Garbage, "Id", "Name", garbageMaterial.IdGarbage);
            ViewData["IdMaterial"] = new SelectList(_context.Material, "Id", "Name", garbageMaterial.IdMaterial);
            return View(garbageMaterial);
        }

        // GET: GarbageMaterials/Edit/5
        public async Task<IActionResult> Edit(int? id, int? IdGarbage)
        {
            if (id == null)
            {
                return NotFound();
            }
            var garbageMaterial = await _context.GarbageMaterial.FindAsync(id);

            if (garbageMaterial == null)
            {
                return NotFound();
            }
            ViewData["IdGarbage"] = new SelectList(_context.Garbage.Where(b => b.Id == IdGarbage), "Id", "Name", garbageMaterial.IdGarbage);
            ViewData["IdMaterial"] = new SelectList(_context.Material, "Id", "Name", garbageMaterial.IdMaterial);
            ViewBag.IdGarbage = IdGarbage;
            ViewBag.IdMaterial = garbageMaterial.IdMaterial;
            ViewBag.GarbageName = _context.Garbage.Where(g => g.Id == IdGarbage).FirstOrDefault().Name;
            
            return View(garbageMaterial);
        }

        // POST: GarbageMaterials/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int IdGarbage, [Bind("Id,IdGarbage,IdMaterial")] GarbageMaterial garbageMaterial)
        {
            if (id != garbageMaterial.Id)
            {
                return NotFound();
            }
            ViewBag.IdGarabge = IdGarbage;
            ViewBag.BrokerId = garbageMaterial.IdMaterial;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(garbageMaterial);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GarbageMaterialExists(garbageMaterial.Id))
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
            ViewData["IdGarbage"] = new SelectList(_context.Garbage, "Id", "Name", garbageMaterial.IdGarbage);
            ViewData["IdMaterial"] = new SelectList(_context.Material, "Id", "Name", garbageMaterial.IdMaterial);

            return RedirectToAction("Index", "GarbageMaterials", new { id = IdGarbage });
        }

        // GET: GarbageMaterials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garbageMaterial = await _context.GarbageMaterial
                .Include(g => g.IdGarbageNavigation)
                .Include(g => g.IdMaterialNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (garbageMaterial == null)
            {
                return NotFound();
            }

            return View(garbageMaterial);
        }

        // POST: GarbageMaterials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int IdGarbage)
        {
            ViewBag.IdGarbage = IdGarbage;
            var garbageMaterial = await _context.GarbageMaterial.FindAsync(id);
            _context.GarbageMaterial.Remove(garbageMaterial);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "GarbageMaterials", new { id = IdGarbage });
        }

        private bool GarbageMaterialExists(int id)
        {
            return _context.GarbageMaterial.Any(e => e.Id == id);
        }
    }
}
