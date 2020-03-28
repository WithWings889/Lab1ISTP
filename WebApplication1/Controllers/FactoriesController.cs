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
    public class FactoriesController : Controller
    {
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
        public async Task<IActionResult> Index(string search = null)
        {
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
                                
                                //перегляд усіх рядків                    
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
                                            _context.Factory.Add(factory);
                                        }
                                        
                                        int i = 4;
                                        //у разі наявності автора знайти його, у разі відсутності - додати
                                        do
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
                                                    //додати в контекст
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
                                        } while (true);
                                    }
                                    catch (Exception e)
                                    {
                                        //viewModel.ErrorsTotal++;

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

        public ActionResult Export(string search = null)
        {
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var factories = _context.Factory.Where(a => a.Name.Contains(search)).ToList();
                //тут, для прикладу ми пишемо усі книжки з БД, в своїх проектах ТАК НЕ РОБИТИ (писати лише вибрані)
                foreach (var t in factories)
                {
                    var worksheet = workbook.Worksheets.Add(search);

                    worksheet.Cell("A1").Value = "Назва";
                    worksheet.Cell("B1").Value = "Адреса";
                    worksheet.Cell("C1").Value = "Вебсайт";
                    worksheet.Row(1).Style.Font.Bold = true;
                    

                    //нумерація рядків/стовпчиків починається з індекса 1 (не 0)
                    for (int i = 0; i < factories.Count; i++)
                    {
                        //worksheet.Cell().Value = "Автор 3";
                        worksheet.Cell(i + 2, 1).Value = factories[i].Name;
                        worksheet.Cell(i + 2, 2).Value = factories[i].Address;
                        worksheet.Cell(i + 2, 3).Value = factories[i].Website;

                        var ft = _context.FactoryGarbageType.Where(a => a.IdFactory == factories[i].Id).Include(a => a.IdGarbageTypeNavigation).ToList();
                        //більше 4-ох нікуди писати
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
