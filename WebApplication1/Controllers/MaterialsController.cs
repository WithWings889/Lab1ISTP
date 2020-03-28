using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1;

namespace WebApplication1.Controllers
{
    public class MaterialsController : Controller
    {
        private readonly RecyclingNowContext _context;

        public MaterialsController(RecyclingNowContext context)
        {
            _context = context;
        }
        public List<Material> SearchMaterial(string search)
        {
            return _context.Material.Where(a => a.Name.Contains(search)).
                                    Include(a => a.GarbageMaterial).
                                    ToList();
        }
        // GET: Materials
        public async Task<IActionResult> Index(string search = null)
        {
            ViewData["CurrentFilter"] = search;
            if (!string.IsNullOrEmpty(search))
            {
                var found = SearchMaterial(search);
                return View(found);
            }
            var recyclingNowContext = _context.Material.Include(m => m.IdGarbageTypeNavigation);
            return View(await recyclingNowContext.ToListAsync());
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
                .Include(m => m.IdGarbageTypeNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            ViewBag.GarbagesLength = _context.GarbageMaterial.Where(e => e.IdMaterial == id).Include(e => e.IdGarbageNavigation).Count();
            ViewBag.GarbageList = _context.GarbageMaterial.Where(e => e.IdMaterial == id).Include(e => e.IdGarbageNavigation).ToList();

            return View(material);
        }

        // GET: Materials/Create
        public IActionResult Create()
        {
            ViewData["IdGarbageType"] = new SelectList(_context.GarbageType, "Id", "Name");
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
            ViewData["IdGarbageType"] = new SelectList(_context.GarbageType, "Id", "Name", material.IdGarbageType);
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
            ViewData["IdGarbageType"] = new SelectList(_context.GarbageType, "Id", "Name", material.IdGarbageType);
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
            ViewData["IdGarbageType"] = new SelectList(_context.GarbageType, "Id", "Name", material.IdGarbageType);
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
                .Include(m => m.IdGarbageTypeNavigation)
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
        public ActionResult Export(string search = null)
        {
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var types = _context.Material.Where(a => a.Name.Contains(search)).Select(a => a.IdGarbageTypeNavigation).Distinct().ToList();
                //тут, для прикладу ми пишемо усі книжки з БД, в своїх проектах ТАК НЕ РОБИТИ (писати лише вибрані)
                foreach (var t in types)
                {
                    var worksheet = workbook.Worksheets.Add(t.Name);

                    worksheet.Cell("A1").Value = "Назва";
                    worksheet.Cell("B1").Value = "";
                    worksheet.Cell("C1").Value = "Інформація";
                    worksheet.Row(1).Style.Font.Bold = true;

                    var materials = _context.Material.Where(a => a.Name.Contains(search)).Where(a => a.IdGarbageType == t.Id).ToList();
                    //нумерація рядків/стовпчиків починається з індекса 1 (не 0)
                    for (int i = 0; i < materials.Count; i++)
                    {
                        //worksheet.Cell().Value = "Автор 3";
                        worksheet.Cell(i + 2, 1).Value = materials[i].Name;
                        worksheet.Cell(i + 2, 2).Value = materials[i].MaterialCard;
                        worksheet.Cell(i + 2, 3).Value = materials[i].Info;

                        //var ft = _context.FactoryGarbageType.Where(a => a.IdFactory == factories[i].Id).Include(a => a.IdGarbageTypeNavigation).ToList();
                        ////більше 4-ох нікуди писати
                        //int j = 0;
                        //foreach (var f in ft)
                        //{
                        //    worksheet.Cell(i + 2, j + 4).Value = f.IdGarbageTypeNavigation.Name;
                        //    j++;

                        //}

                    }
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"Materials_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }
    }
}
