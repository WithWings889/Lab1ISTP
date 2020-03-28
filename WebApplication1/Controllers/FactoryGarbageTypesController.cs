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
            var recyclingNowContext = _context.FactoryGarbageType.Where(e => e.IdFactory == id).Include(f => f.IdFactoryNavigation).Include(f => f.IdGarbageTypeNavigation);
            return View(await recyclingNowContext.ToListAsync());
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
                .Include(f => f.IdFactoryNavigation)
                .Include(f => f.IdGarbageTypeNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (factoryGarbageType == null)
            {
                return NotFound();
            }
            ViewBag.Count = _context.FactoryGarbageType.Where(b => b.IdGarbageType == idGarbageType).Include(b => b.IdFactoryNavigation).Count();
            ViewBag.CategoriesList = _context.FactoryGarbageType.Where(b => b.IdGarbageType == idGarbageType).Include(b => b.IdFactoryNavigation).ToList();
            return View(factoryGarbageType);
        }

        // GET: FactoryGarbageTypes/Create
        public IActionResult Create(int IdFactory)
        {
            ViewBag.FactoryId = IdFactory;
            ViewData["IdFactory"] = new SelectList(_context.Factory.Where(b => b.Id == IdFactory), "Id", "Name");
            ViewData["IdGarbageType"] = new SelectList(_context.GarbageType, "Id", "Name");
            return View();
        }

        // POST: FactoryGarbageTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int IdFactory, [Bind("Id,IdGarbageType,IdFactory")] FactoryGarbageType factoryGarbageType)
        {
            factoryGarbageType.IdFactory = IdFactory;
            if (ModelState.IsValid)
            {
                _context.Add(factoryGarbageType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "FactoryGarbageTypes", new { id = IdFactory });
            }
            ViewData["IdFactory"] = new SelectList(_context.Factory, "Id", "Name", factoryGarbageType.IdFactory);
            ViewData["IdGarbageType"] = new SelectList(_context.GarbageType, "Id", "Name", factoryGarbageType.IdGarbageType);
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
            ViewData["IdFactory"] = new SelectList(_context.Factory.Where(b => b.Id == IdFactory), "Id", "Name", factoryGarbageType.IdFactory);
            ViewData["IdGarbageType"] = new SelectList(_context.GarbageType, "Id", "Name", factoryGarbageType.IdGarbageType);
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
            ViewData["IdFactory"] = new SelectList(_context.Factory, "Id", "Name", factoryGarbageType.IdFactory);
            ViewData["IdGarbageType"] = new SelectList(_context.GarbageType, "Id", "Name", factoryGarbageType.IdGarbageType);
            return RedirectToAction("Index", "FactoryGarbageTypes", new { id = IdFactory }); ;
        }

        // GET: FactoryGarbageTypes/Delete/5
        public async Task<IActionResult> Delete(int? id, int? IdFactory)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factoryGarbageType = await _context.FactoryGarbageType
                .Include(f => f.IdFactoryNavigation)
                .Include(f => f.IdGarbageTypeNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (factoryGarbageType == null)
            {
                return NotFound();
            }

            return View(factoryGarbageType);
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
