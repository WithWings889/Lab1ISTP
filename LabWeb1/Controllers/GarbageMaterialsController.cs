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
            var materialInGarbage = _context.GarbageMaterial.Where(e => e.IdGarbage == id).Include(e => e.Material);
            return View(materialInGarbage.ToList());
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
            var garbageMaterials = await _context.GarbageMaterial
                .Include(b => b.Material)
                .Include(b => b.Garbage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (garbageMaterials == null)
            {
                return NotFound();
            }
            ViewBag.GarbageName = garbageMaterials.Garbage.Name;
            ViewBag.Count = _context.GarbageMaterial.Where(b => b.IdMaterial == IdMaterial).Include(b => b.Garbage).Count();
            ViewBag.GarbagesList = _context.GarbageMaterial.Where(b => b.IdMaterial == IdMaterial).Include(b => b.Garbage).ToList();
            return View(garbageMaterials);
        }

        // GET: GarbageMaterials/Create
        public IActionResult Create(int idGarbage)
        {
            ViewBag.IdGarbage = idGarbage;
            ViewBag.GarbageName = _context.Garbage.Where(g => g.Id == idGarbage).FirstOrDefault().Name;
            ViewData["IdMaterials"] = new SelectList(_context.Material, "Id", "Name");
            ViewData["IdGarbages"] = new SelectList(_context.Garbage.Where(b => b.Id ==idGarbage), "Id", "Name");
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
            ViewBag.IdGarbage = IdGarbage;
            ViewBag.GarbageName = _context.Garbage.Where(g => g.Id == IdGarbage).FirstOrDefault().Name;
            ViewData["IdMaterials"] = new SelectList(_context.Material, "Id", "Name", garbageMaterial.IdMaterial);
            ViewBag.IdMaterial = garbageMaterial.IdMaterial;
            ViewData["IdGarbages"] = new SelectList(_context.Garbage.Where(b => b.Id == IdGarbage), 
                "Id", "Name", garbageMaterial.IdGarbage);
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
            return RedirectToAction("Index", "GarbageMaterials", new { id = IdGarbage });
        }

        // GET: GarbageMaterials/Delete/5
        public async Task<IActionResult> Delete(int? id, int? IdGarbage)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.GarbageName = _context.Garbage.Where(g => g.Id == IdGarbage).FirstOrDefault().Name;
            var garbageMaterial = await _context.GarbageMaterial
                .Include(b => b.Material)
                .Include(b => b.Garbage)
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
