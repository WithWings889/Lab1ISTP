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
    public class GarbageTypesController : Controller
    {
        private readonly RecyclingNowContext _context;

        public GarbageTypesController(RecyclingNowContext context)
        {
            _context = context;
        }

        // GET: GarbageTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.GarbageType.ToListAsync());
        }
        public async Task<IActionResult> IndexForFactory(int? id)
        {
            ViewBag.IdFactory = id;
            ViewBag.FactoryName = _context.Factory.Find(id).Name;
            var garbageTypesInFactory = _context.FactoryGarbageType.Where(e => e.IdFactory == id).Select(e => e.GarbageType);
            return View(await garbageTypesInFactory.ToListAsync());
        }

        // GET: GarbageTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.Factory = "Фабрики";
            var garbageType = await _context.GarbageType
                .FirstOrDefaultAsync(m => m.Id == id);
            ViewBag.FactoriesLength = _context.FactoryGarbageType.Where(e => e.IdGarbageType == id).Include(e => e.Factory).Count();
            ViewBag.FactoryList = _context.FactoryGarbageType.Where(e => e.IdGarbageType == id).Include(e => e.Factory).ToList();
            if (garbageType == null)
            {
                return NotFound();
            }

            return View(garbageType);
        }

        // GET: GarbageTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GarbageTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] GarbageType garbageType)
        {

            if (ModelState.IsValid)
            {
                var garbagetype = _context.GarbageType.Where(g => g.Name == garbageType.Name).FirstOrDefault();
                if (garbagetype != null)
                {
                    ModelState.AddModelError(string.Empty, "Даний тип сміття вже існує");
                }
                else
                {
                    _context.Add(garbageType);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(garbageType);
        }

        // GET: GarbageTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garbageType = await _context.GarbageType.FindAsync(id);
            if (garbageType == null)
            {
                return NotFound();
            }
            return View(garbageType);
        }

        // POST: GarbageTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] GarbageType garbageType)
        {
            if (id != garbageType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(garbageType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GarbageTypeExists(garbageType.Id))
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
            return View(garbageType);
        }

        // GET: GarbageTypes/Delete/5
        public async Task<IActionResult> Delete(int? id, bool saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var garbageType = await _context.GarbageType
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (garbageType == null)
            {
                return NotFound();
            }
            if (saveChangesError == true)
            {
                ViewData["ErrorMessage"] = "Помилка видалення";
            }

            return View(garbageType);
        }

        // POST: GarbageTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var garbageType = await _context.GarbageType.FindAsync(id);
            if (garbageType == null) return RedirectToAction(nameof(Index));

            try
            {
                DeleteGarbageTypeInMaterials(id);
                DeleteGarbageTypeInFactory(id);
                _context.GarbageType.Remove(garbageType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        private bool GarbageTypeExists(int id)
        {
            return _context.GarbageType.Any(e => e.Id == id);
        }
        private async void DeleteGarbageTypeInMaterials(int id)
        {
            var garbageType = await _context.GarbageType.FindAsync(id);

            foreach (var i in _context.Material)
            {
                if (i.IdGarbageType == garbageType.Id) i.IdGarbageType = null;
            }
        }

        private async void DeleteGarbageTypeInFactory(int id)
        {
            var garbageType = await _context.GarbageType.FindAsync(id);

            foreach (var i in _context.FactoryGarbageType)
            {
                if (i.IdGarbageType == garbageType.Id)
                    _context.FactoryGarbageType.Remove(i);
            }
        }
    }
}
