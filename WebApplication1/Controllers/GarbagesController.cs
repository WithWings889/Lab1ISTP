using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1;

namespace WebApplication1.Controllers
{
    public class GarbagesController : Controller
    {
        private readonly RecyclingNowContext _context;

        public GarbagesController(RecyclingNowContext context)
        {
            _context = context;
        }
        public List<Garbage> SearchGarbages(string search)
        {
            return _context.Garbage.Where(a => a.Name.Contains(search)).Include(a => a.GarbageMaterial).ToList();
        }

        // GET: Garbages
        public async Task<IActionResult> Index(string search = null)
        {
            ViewData["CurrentFilter"] = search;
            if (!string.IsNullOrEmpty(search))
            {
                var found = SearchGarbages(search);
                return View(found);
            }
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile fileExcel)
        {
            if (ModelState.IsValid)
            {
                if (fileExcel != null)
                {
                    using (var stream = new FileStream(fileExcel.FileName, FileMode.Create))
                    {
                        await fileExcel.CopyToAsync(stream);
                        using (XLWorkbook workBook = new XLWorkbook(stream, XLEventTracking.Disabled))
                        {
                            //перегляд усіх листів (в даному випадку категорій)
                            foreach (IXLWorksheet worksheet in workBook.Worksheets)
                            {
                                //worksheet.Name - назва категорії. Пробуємо знайти в БД, якщо відсутня, то створюємо нову
                                GarbageType newtype;
                                var t = (from typ in _context.GarbageType
                                         where typ.Name.Contains(worksheet.Name)
                                         select typ).ToList();
                                if (t.Count > 0)
                                {
                                    newtype = t[0];
                                }
                                else
                                {
                                    newtype = new GarbageType();
                                    newtype.Name = worksheet.Name;
                                    _context.GarbageType.Add(newtype);
                                }
                                //перегляд усіх рядків                    
                                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                                {
                                    try
                                    {
                                        Garbage garbage = new Garbage();
                                        garbage.Name = row.Cell(1).Value.ToString();
                                        _context.Garbage.Add(garbage);
                                        //у разі наявності автора знайти його, у разі відсутності - додати
                                        for (int i = 2; i <= 5; i++)
                                        {
                                            if (row.Cell(i).Value.ToString().Length > 0)
                                            {
                                                Material material;

                                                var a = (from aut in _context.Material
                                                         where aut.Name.Contains(row.Cell(i).Value.ToString())
                                                         select aut).ToList();
                                                if (a.Count > 0)
                                                {
                                                    material = a[0];
                                                    GarbageMaterial gm = new GarbageMaterial();
                                                    gm.IdGarbageNavigation = garbage;
                                                    gm.IdMaterialNavigation = material;
                                                    _context.GarbageMaterial.Add(gm);
                                                }
                                                else
                                                {
                                                    
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        //logging самостійно :)

                                    }
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
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

        int MaxLengthMaterials(List<Garbage> garbage, string search)
        {
            //var garbage = _context.Garbage.Where(g => g.Name.Contains(search)).Include(g => g.GarbageMaterial).ToList();
            int max = garbage[0].GarbageMaterial.Count();
            for(int i = 1; i < garbage.Count; ++i)
            {
                if (max < garbage[i].GarbageMaterial.Count)
                    max = garbage[i].GarbageMaterial.Count;
            }
            return max;
        }
        public ActionResult Export(string search = null)
        {
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var types = _context.GarbageMaterial.Where(a => a.IdGarbageNavigation.Name.Contains(search)).
                                                     Select(a => a.IdMaterialNavigation.IdGarbageTypeNavigation).
                                                     Distinct().
                                                     ToList();
                
                //тут, для прикладу ми пишемо усі книжки з БД, в своїх проектах ТАК НЕ РОБИТИ (писати лише вибрані)
                foreach (var t in types)
                {
                    var worksheet = workbook.Worksheets.Add(t.Name);

                    var garbages = _context.Garbage.Where(a => a.Name.Contains(search)).Include(a => a.GarbageMaterial).ToList();

                    worksheet.Cell("A1").Value = "Назва";
                    int length = MaxLengthMaterials(garbages, search);
                    for (int i = 0; i < length; ++i)
                    {
                        worksheet.Cell(1, i + 2).Value = "Матеріал" + i.ToString();
                    }
                    worksheet.Row(1).Style.Font.Bold = true;

                   
                    //нумерація рядків/стовпчиків починається з індекса 1 (не 0)
                    for (int i = 0; i < garbages.Count; i++)
                    {
                        //worksheet.Cell().Value = "Автор 3";
                        worksheet.Cell(i + 2, 1).Value = garbages[i].Name;

                        var gm = _context.GarbageMaterial.Where(a => a.IdGarbage == garbages[i].Id).Include(a => a.IdMaterialNavigation).ToList();
                        //більше 4-ох нікуди писати
                        int j = 0;
                        foreach (var m in gm)
                        {
                            worksheet.Cell(i + 2, j + length + 1).Value = m.IdMaterialNavigation.Name;
                            j++;

                        }

                    }
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"Garbages_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }
    }
}
