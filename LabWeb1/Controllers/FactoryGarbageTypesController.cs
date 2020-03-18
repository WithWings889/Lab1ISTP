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
    public class FactoryGarbageTypesController : Controller
    {
        private readonly RecyclingNowContext _context;

        public FactoryGarbageTypesController(RecyclingNowContext context)
        {
            _context = context;
        }

        // GET: FactoryGarbageTypes

        public async Task<IActionResult> Index(int? id)
        {
            ViewBag.IdFactory = id;
            ViewBag.FactoryName = _context.Factory.Find(id).Name;
            var garbageTypesInFactory = _context.FactoryGarbageType.Where(e => e.IdFactory == id).Include(e => e.GarbageType);
            return View(garbageTypesInFactory.ToList());
        }

        // GET: FactoryGarbageTypes/Details/5
        public async Task<IActionResult> Details(int? id, int? idFactory)
        {
            ViewBag.CategoryId = idFactory;
            if (id == null)
            {
                return NotFound();
            }
            var idGarbageType = _context.FactoryGarbageType.Find(id).IdGarbageType;
            var factoryGarbageType = await _context.FactoryGarbageType
                .Include(b => b.GarbageType)
                .Include(b => b.Factory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (factoryGarbageType == null)
            {
                return NotFound();
            }
            ViewBag.Count = _context.FactoryGarbageType.Where(b => b.IdGarbageType == idGarbageType).Include(b => b.Factory).Count();
            ViewBag.CategoriesList = _context.FactoryGarbageType.Where(b => b.IdGarbageType == idGarbageType).Include(b => b.Factory).ToList();
            return View(factoryGarbageType);
        }

        // GET: FactoryGarbageTypes/Create
        public IActionResult Create(int IdFactory)
        {
            ViewBag.FactoryId = IdFactory;
            ViewData["IdGarbageTypes"] = new SelectList(_context.GarbageType, "Id", "Name");
            ViewData["IdFactories"] = new SelectList(_context.Factory.Where(b => b.Id == IdFactory), "Id", "Name");
            return View();
        }

        // POST: FactoryGarbageTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int IdFactory, int IdGarbageType, [Bind("Id,IdGarbageType,IdFactory")] FactoryGarbageType factoryGarbageType)
        {
            factoryGarbageType.IdFactory = IdFactory;
            if (ModelState.IsValid)
            {
                _context.Add(factoryGarbageType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "FactoryGarbageTypes", new { id = IdFactory });
               // new { id = IdFactory, name = _context.Factory.Where(c => c.Id == IdFactory).FirstOrDefault().Name });
            }
            return View(factoryGarbageType);
        }

        // GET: FactoryGarbageTypes/Edit/5
        public async Task<IActionResult> Edit(int? id, int? IdFactory)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factoryGarbageType = await _context.FactoryGarbageType.FindAsync(id);
            if (factoryGarbageType == null)
            {
                return NotFound();
            }
            ViewBag.IdFactory = IdFactory;
            ViewData["IdGarbageTypes"] = new SelectList(_context.GarbageType, "Id", "Name", factoryGarbageType.IdGarbageType);

            ViewBag.IdGarbageType = factoryGarbageType.IdGarbageType;
            ViewData["IdFactories"] = new SelectList(_context.Factory.Where(b => b.Id == IdFactory), "Id", "Factory", factoryGarbageType.IdFactory);
            return View(factoryGarbageType);
        }

        // POST: FactoryGarbageTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int IdFactory, [Bind("Id,IdGarbageType,IdFactory")] FactoryGarbageType factoryGarbageType)
        {
            if (id != factoryGarbageType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(factoryGarbageType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FactoryGarbageTypeExists(factoryGarbageType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "FactoryGarbageTypes", new { id = IdFactory });
            }
            return RedirectToAction("Index", "FactoryGarbageTypes", new { id = IdFactory });
        }

        // GET: FactoryGarbageTypes/Delete/5
        public async Task<IActionResult> Delete(int? id, int? IdFactory)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factoryGarbageTypes = await _context.FactoryGarbageType
                .Include(b => b.GarbageType)
                .Include(b => b.Factory)
                .FirstOrDefaultAsync(m => m.Id == id);
            ViewBag.IdFactory = IdFactory;
            if (factoryGarbageTypes == null)
            {
                return NotFound();
            }

            return View(factoryGarbageTypes);
        }

        // POST: FactoryGarbageTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int IdFactory)
        {
            var factoryGarbageType = await _context.FactoryGarbageType.FindAsync(id);
            _context.FactoryGarbageType.Remove(factoryGarbageType);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "FactoryGarbageTypes", new { id = IdFactory });
        }

        private bool FactoryGarbageTypeExists(int id)
        {
            return _context.FactoryGarbageType.Any(e => e.Id == id);
        }
    }
}
