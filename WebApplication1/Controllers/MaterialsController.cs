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
                                    Include(a => a.IdGarbageTypeNavigation).
                                    ToList();
        }
        // GET: Materials
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
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                                    if (TryValidateModel(newtype))
                                        _context.GarbageType.Add(newtype);
                                    else ++errorCount;
                                }
                                //перегляд усіх рядків                    
                                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                                {
                                    try
                                    {
                                        Material material;

                                        var g = (from mat in _context.Material
                                                 where mat.Name.Contains(row.Cell(1).Value.ToString())
                                                 select mat).ToList();
                                        if (g.Count > 0)
                                        {
                                            material = g[0];
                                            errorCount++;
                                        }
                                        else
                                        {
                                            material = new Material();
                                            material.Name = row.Cell(1).Value.ToString();
                                            if (TryValidateModel(material, nameof(Material)))
                                                _context.Material.Add(material);
                                            else
                                                ++errorCount;
                                        }
                                        //у разі наявності автора знайти його, у разі відсутності - додати
                                        for (int i = 2; i <= 5; i++)
                                        {
                                            if (row.Cell(i).Value.ToString().Length > 0)
                                            {
                                                Garbage garbage;

                                                var a = (from gar in _context.Garbage
                                                         where gar.Name.Contains(row.Cell(i).Value.ToString())
                                                         select gar).ToList();
                                                if (a.Count > 0)
                                                {
                                                    garbage = a[0];
                                                   
                                                }
                                                else
                                                {
                                                    garbage = new Garbage();
                                                    garbage.Name = row.Cell(i).Value.ToString();
                                                    if (TryValidateModel(garbage, nameof(Garbage)))
                                                    {
                                                        _context.Garbage.Add(garbage);
                                                        GarbageMaterial gm;
                                                        var b = (from garmat in _context.GarbageMaterial
                                                                 where garmat.IdGarbageNavigation.Name == garbage.Name &&
                                                                       garmat.IdMaterialNavigation.Name == material.Name
                                                                 select garmat).ToList();
                                                        if (a.Count > 0)
                                                        {
                                                            errorCount++;
                                                        }
                                                        else
                                                        {
                                                            gm = new GarbageMaterial();
                                                            gm.IdGarbageNavigation = garbage;
                                                            gm.IdMaterialNavigation = material;
                                                            _context.GarbageMaterial.Add(gm);
                                                        }
                                                    }
                                                    else
                                                        ++errorCount;
                                                }
                                                
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        ++errorCount;
                                    }
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { errorCount = errorCount});
        }
        public ActionResult Export(string search = null)
        {
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var types = _context.Material.Where(a => a.Name.Contains(search)).Select(a => a.IdGarbageTypeNavigation).Distinct().ToList();
                
                foreach (var t in types)
                {
                    var worksheet = workbook.Worksheets.Add(t.Name);

                    worksheet.Cell("A1").Value = "Назва";
                    worksheet.Cell("B1").Value = "Індекс матеріалу";
                    worksheet.Cell("C1").Value = "Інформація";
                    worksheet.Row(1).Style.Font.Bold = true;

                    var materials = _context.Material.Where(a => a.Name.Contains(search)).Where(a => a.IdGarbageType == t.Id).ToList();

                    for (int i = 0; i < materials.Count; i++)
                    {

                        worksheet.Cell(i + 2, 1).Value = materials[i].Name;
                        worksheet.Cell(i + 2, 2).Value = materials[i].MaterialCard;
                        worksheet.Cell(i + 2, 3).Value = materials[i].Info;

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
