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
    public class MaterialsController : Controller
    {
        private readonly RecyclingNowContext _context;

        public MaterialsController(RecyclingNowContext context)
        {
            _context = context;
        }

        // GET: Materials
        public async Task<IActionResult> Index()
        {
            var materials = _context.Material.Include(m => m.GarbageType);
            return View(await materials.ToListAsync());
        }
        

        // GET: Materials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.Garbage = "Сміття";
            var material = await _context.Material
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }
            ViewBag.GarbagesLength = _context.GarbageMaterial.Where(e => e.IdMaterial == id).Include(e => e.Garbage).Count();
            ViewBag.GarbageList = _context.GarbageMaterial.Where(e => e.IdMaterial == id).Include(e => e.Garbage).ToList();
            return View(material);
        }

        // GET: Materials/Create
        public IActionResult Create()
        {
            ViewData["IdGarbageTypes"] = new SelectList(_context.GarbageType, "Id", "Name");
            return View();
        }

        // POST: Materials/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,MaterialCard,IdGarbageType,Info")] Material material)
        {
            if (ModelState.IsValid)
            {
                var m = _context.Material.Where(c => c.MaterialCard == material.MaterialCard).FirstOrDefault();
                if (m != null)
                {
                    ModelState.AddModelError(string.Empty, "Матеріал з таким ідентифікатором вже існує");
                }
                _context.Add(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(material);
        }

        // GET: Materials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Material.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }
            return View(material);
        }

        // POST: Materials/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,MaterialCard,IdGarbageType,Info")] Material material)
        {
            if (id != material.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(material);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(material.Id))
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
            return View(material);
        }

        // GET: Materials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Material
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // POST: Materials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Material.FindAsync(id);
            DeleteMaterialInGarbage(id);
            _context.Material.Remove(material);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaterialExists(int id)
        {
            return _context.Material.Any(e => e.Id == id);
        }
        private async void DeleteMaterialInGarbage(int id)
        {
            var material = await _context.Material.FindAsync(id);

            foreach (var i in _context.GarbageMaterial)
            {
                if (i.IdMaterial == material.Id)
                    _context.GarbageMaterial.Remove(i);
            }
        }
    }
}
