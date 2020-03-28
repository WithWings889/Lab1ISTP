using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartFactoryGarbageTypeController : ControllerBase
    {
        private readonly RecyclingNowContext _context;
        public ChartFactoryGarbageTypeController(RecyclingNowContext context)
        {
            _context = context;
        }
        [HttpGet("JsonData")]
        public JsonResult JsonData()
        {
            //List<object> fabgarbageType = new List<object>();
            //fabgarbageType.Add(new[] { "Фабрика", "Кількість типів сміття на переробку" });
            //foreach (var g in garbageTypesFac)
            //{
            //    fabgarbageType.Add(new object[] { g.Name, g.FactoryGarbageType.Count() });
            //}
            //return new JsonResult(fabgarbageType);

            var garbageTypesFac = _context.Factory.Include(a => a.FactoryGarbageType).ToList();
            var garbageTypes = _context.GarbageType.Include(a => a.FactoryGarbageType).ToList();

            List<object> temp  = new List<object>();
            List<List<object>> fabgarbageType = new List<List<object>>();

            
            temp.Add("Типи сміття");
            foreach (var g in garbageTypes)
            {
                temp.Add(g.Name);
            }
            fabgarbageType.Add(temp);
            temp = new List<object>();

            foreach (var f in garbageTypesFac)
            {
                temp.Add(f.Name);
                foreach (var g in garbageTypes)
                {
                    var flag= f.FactoryGarbageType.Where(a => a.IdGarbageType == g.Id).ToList();
                    if (flag.Count() == 0)
                        temp.Add(0);
                    else
                        temp.Add(1);
                }
                fabgarbageType.Add(temp);
                temp = new List<object>();
            }

            return new JsonResult(fabgarbageType);
        }
    }
}