using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1;

namespace WebApplication1.Controllers
{
    public class GarbageTypesController : Controller
    {
        private readonly RecyclingNowContext _context;

        public GarbageTypesController(RecyclingNowContext context)
        {
            _context = context;
        }
        public List<GarbageType> SearchGarbageType(string search = null)
        {
            return _context.GarbageType.Where(a => a.Name.Contains(search)).
                                        Include(a => a.Material).
                                        Include(a => a.FactoryGarbageType).
                                        ToList();
        }
        // GET: GarbageTypes
        public async Task<IActionResult> Index(string search = null, int errorCount = 0)
        {
            var roles = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            if (roles.Contains("admin"))
                ViewBag.Admin = true;
            else ViewBag.Admin = false;
            if (roles.Contains("user"))
                ViewBag.User = true;
            else ViewBag.User = false;

            ViewBag.Error = errorCount;
            ViewData["CurrentFilter"] = search;
            if (!string.IsNullOrEmpty(search))
            {
                var found = SearchGarbageType(search);
                return View(found);
            }
            return View(await _context.GarbageType.ToListAsync());
        }

        // GET: GarbageTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.Factory = "Фабрики";
            ViewBag.FactoriesLength = _context.FactoryGarbageType.Where(e => e.IdGarbageType == id).Include(e => e.IdFactoryNavigation).Count();
            ViewBag.FactoryList = _context.FactoryGarbageType.Where(e => e.IdGarbageType == id).Include(e => e.IdFactoryNavigation).ToList();
            var garbageType = await _context.GarbageType
                .FirstOrDefaultAsync(m => m.Id == id);
            if (garbageType == null)
            {
                return NotFound();
            }

            return View(garbageType);
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

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile fileExcel)
        {
            int errorCount = 0;
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
                                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                                {
                                    GarbageType newtype;
                                    var t = (from typ in _context.GarbageType
                                             where typ.Name.Contains(row.Cell(1).Value.ToString())
                                             select typ).ToList();
                                    if (t.Count > 0)
                                    {
                                        newtype = t[0];
                                    }
                                    else
                                    {
                                        newtype = new GarbageType();
                                        newtype.Name = row.Cell(1).Value.ToString();
                                        if (TryValidateModel(newtype, nameof(GarbageType)))
                                            _context.GarbageType.Add(newtype);
                                        else
                                        {
                                            ++errorCount;
                                            continue;
                                        }
                                    }
                                    int i = 2;
                                    //while(true)
                                    for(i = 2; i < 5; ++i)
                                    {
                                        if (row.Cell(i).Value.ToString().Length != 0)
                                        {
                                            try
                                            {
                                                Material material;
                                                var m = (from mat in _context.Material
                                                         where mat.Name.Contains(row.Cell(1).Value.ToString())
                                                         select mat).ToList();
                                                if (m.Count > 0)
                                                {
                                                    material = m[0];
                                                }
                                                else
                                                {
                                                    errorCount++;
                                                }

                                            }
                                            catch (Exception e)
                                            {
                                                ++errorCount;
                                            }
                                        }
                                        else
                                            break;

                                    }
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }

            }
            return RedirectToAction(nameof(Index), new { errorCount = errorCount });
        }
        int MaxLengthMaterials(List<GarbageType> garbageType)
        {
            //var garbage = _context.Garbage.Where(g => g.Name.Contains(search)).Include(g => g.GarbageMaterial).ToList();
            int max = garbageType[0].Material.Count();
            for (int i = 1; i < garbageType.Count; ++i)
            {
                if (max < garbageType[i].Material.Count)
                    max = garbageType[i].Material.Count;
            }
            return max;
        }
        public ActionResult Export(string search = null)
        {
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                List<GarbageType> types;
                if (search == null)
                {
                    types = _context.GarbageType.Include(a => a.Material).ToList();
                }
                else
                    types = _context.GarbageType.Where(a => a.Name.Contains(search)).Include(a => a.Material).Distinct().ToList();
                var worksheet = workbook.Worksheets.Add("Типи сміття");

                if (types.Count() == 0)
                {
                    worksheet.Cell("A1").Value = "За даними параметрами не знайдено жодного типу сміття";
                }
                else
                {
                    int length = MaxLengthMaterials(types);

                    worksheet.Cell("A1").Value = "Назва";

                    for (int i = 0; i < length; ++i)
                    {
                        worksheet.Cell(1, i + 2).Value = "Матеріал" + (i + 1).ToString();
                    }
                    worksheet.Row(1).Style.Font.Bold = true;


                    for (int i = 0; i < types.Count; i++)
                    {
                        //worksheet.Cell().Value = "Автор 3";
                        worksheet.Cell(i + 2, 1).Value = types[i].Name;

                        var mt = _context.Material.Where(a => a.IdGarbageType == types[i].Id).ToList();
                        //більше 4-ох нікуди писати
                        int j = 0;
                        foreach (var m in mt)
                        {
                            worksheet.Cell(i + 2, j + 2).Value = m.Name;
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
                        FileDownloadName = $"GarbageType_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }

        }
    }
}
