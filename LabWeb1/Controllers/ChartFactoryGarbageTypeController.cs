using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabWeb1.Controllers
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
            var garbageTypesFac = _context.Factory.Include(a => a.FactoryGarbageTypes).ToList();
            //var garbageTypes = _context.Factory.Include(a => a.GarbageTypes).ToList();

            List<object> fabgarbageType = new List<object>();
            List<object> temp = new List<object>();

            temp.Add("Типи сміття");
            foreach (var g in garbageTypesFac)
            {
                temp.Add(g.FactoryGarbageType.GarbageType.Name);
            }
            fabgarbageType.Add(temp);
            temp.Clear();

            foreach (var f in garbageTypesFac)
            {
                temp.Add(f.FactoryGarbageType.Factory.Name);
                foreach (var g in garbageTypesFac)
                {
                    var flag = _context.FactoryGarbageType
                        .Where(el => el.IdFactory == f.FactoryGarbageType.IdFactory)
                        .Where(el => el.IdGarbageType == g.FactoryGarbageType.IdGarbageType);
                    if (flag == null)
                        temp.Add(0);
                    temp.Add(1);
                }
                fabgarbageType.Add(temp);
                temp.Clear();
            }

            return new JsonResult(fabgarbageType);
        }
    }
}