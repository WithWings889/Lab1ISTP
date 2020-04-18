using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class FactoriesController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly RecyclingNowContext _context;

        public FactoriesController(RecyclingNowContext context)
        {
            _context = context;
        }
        public List<Factory> SearchFactory(string search)
        {
            return _context.Factory.Where(a => a.Name.Contains(search) ||
                                    a.Website.Contains(search) ||
                                    a.Address.Contains(search)
                                    ).ToList();
        }

        // GET: Factories
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
                var found = SearchFactory(search);
                return View(found);
            }
            return View(await _context.Factory.ToListAsync());
        }
        [HttpPost]

        public async Task<IActionResult> IndexWithSearch(string search = null)
        {
            if (!string.IsNullOrEmpty(search))
            {
                var found = SearchFactory(search);
                return View(found);
            }
            return View(await _context.Factory.ToListAsync());
        }

        // GET: Factories/Details/5
        public async Task<IActionResult> Details(int? id)
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

            return View(factory);
        }

        // GET: Factories/Create
        [Authorize(Roles="admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Factories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Website")] Factory factory)
        {
            if (ModelState.IsValid)
            {
                var f = _context.Factory.Where(c => c.Name == factory.Name && c.Address == factory.Address).FirstOrDefault();
                if (f != null)
                {
                    ModelState.AddModelError(string.Empty, "Фабрика з такою назвою та за такою ж адресою вже існує");
                }
                _context.Add(factory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(factory);
        }

        // GET: Factories/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factory = await _context.Factory.FindAsync(id);
            if (factory == null)
            {
                return NotFound();
            }
            return View(factory);
        }

        // POST: Factories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Website")] Factory factory)
        {
            if (id != factory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(factory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FactoryExists(factory.Id))
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
            return View(factory);
        }

        // GET: Factories/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
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

            return View(factory);
        }

        // POST: Factories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var factory = await _context.Factory.FindAsync(id);
            DeleteFactoryInGarbageType(id);
            _context.Factory.Remove(factory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private async void DeleteFactoryInGarbageType(int id)
        {
            var factory = await _context.Factory.FindAsync(id);

            foreach (var i in _context.FactoryGarbageType)
            {
                if (i.IdFactory == factory.Id)
                    _context.FactoryGarbageType.Remove(i);
            }
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

        private bool FactoryExists(int id)
        {
            return _context.Factory.Any(e => e.Id == id);
        }
        [Authorize(Roles ="admin")]
        [HttpPost]
       // [ValidateAntiForgeryToken]
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
                            foreach (IXLWorksheet worksheet in workBook.Worksheets)
                            {
                                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                                {
                                    try
                                    {
                                        Factory factory;
                                        var f = (from fab in _context.Factory
                                                 where fab.Name.Contains(row.Cell(1).Value.ToString())
                                                 select fab).ToList();
                                        if (f.Count > 0)
                                        {
                                            factory = f[0];
                                        }
                                        else
                                        {
                                            factory = new Factory();
                                            factory.Name = row.Cell(1).Value.ToString();
                                            factory.Website = row.Cell(2).Value.ToString();
                                            factory.Address = row.Cell(3).Value.ToString();
                                            if(TryValidateModel(factory, nameof(Factory)))
                                                _context.Factory.Add(factory);
                                            else
                                            {
                                                errorCount++;
                                                continue;
                                            }
                                        }

                                        int i = 4;
                                        while(true)
                                        {

                                            if (row.Cell(i).Value.ToString().Length != 0)
                                            {
                                                GarbageType type;

                                                var t = (from typ in _context.GarbageType
                                                         where typ.Name.Contains(row.Cell(i).Value.ToString())
                                                         select typ).ToList();
                                                if (t.Count > 0)
                                                {
                                                    type = t[0];
                                                }
                                                else
                                                {
                                                    type = new GarbageType();
                                                    type.Name = row.Cell(i).Value.ToString();
                                                    if (!TryValidateModel(type, nameof(GarbageType)))
                                                    {
                                                        errorCount++;
                                                    }
                                                    _context.Add(type);
                                                }
                                                FactoryGarbageType ft = new FactoryGarbageType();
                                                ft.IdFactoryNavigation = factory;
                                                ft.IdGarbageTypeNavigation = type;
                                                _context.FactoryGarbageType.Add(ft);
                                                ++i;

                                            }
                                            else
                                                break;
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
            return RedirectToAction(nameof(Index), new { errorCount = errorCount });
        }
        int MaxLengthTypes(List<Factory> factory)
        {
            //var garbage = _context.Garbage.Where(g => g.Name.Contains(search)).Include(g => g.GarbageMaterial).ToList();
            int max = factory[0].FactoryGarbageType.Count();
            for (int i = 1; i < factory.Count; ++i)
            {
                if (max < factory[i].FactoryGarbageType.Count)
                    max = factory[i].FactoryGarbageType.Count;
            }
            return max;
        }

        public ActionResult Export(string search = null)
        {
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                List<Factory> factories;
                if (search == null)
                {
                    factories = _context.Factory.Include(f => f.FactoryGarbageType).ToList();
                    search = "Фабрики";
                }
                else
                    factories = _context.Factory.Where(a => a.Name.Contains(search)).Include(f => f.FactoryGarbageType).Distinct().ToList();
                var worksheet = workbook.Worksheets.Add(search);

                if (factories.Count() == 0)
                {
                    worksheet.Cell("A1").Value = "За даними параметрами не знайдено жодної фабрики";
                }
                else
                {
                    int length = MaxLengthTypes(factories);

                    worksheet.Cell("A1").Value = "Назва";
                    worksheet.Cell("B1").Value = "Адреса";
                    worksheet.Cell("C1").Value = "Вебсайт";
                    for (int i = 0; i < length; ++i)
                    {
                        worksheet.Cell(1, i + 4 ).Value = "Тип сміття" + (i + 1).ToString();
                    }
                    worksheet.Row(1).Style.Font.Bold = true;


                    for (int i = 0; i < factories.Count; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = factories[i].Name;
                        worksheet.Cell(i + 2, 2).Value = factories[i].Address;
                        worksheet.Cell(i + 2, 3).Value = factories[i].Website;

                        var ft = _context.FactoryGarbageType.Where(a => a.IdFactory == factories[i].Id).Include(a => a.IdGarbageTypeNavigation).ToList();
                        
                        int j = 0;
                        foreach (var f in ft)
                        {
                            worksheet.Cell(i + 2, j + 4).Value = f.IdGarbageTypeNavigation.Name;
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
                        FileDownloadName = $"Factories_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }

    }
}
