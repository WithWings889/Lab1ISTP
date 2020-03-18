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
    public class ChartsController : ControllerBase
    {
        private readonly RecyclingNowContext _context;
        public ChartsController(RecyclingNowContext context)
        {
            _context = context;
        }
        [HttpGet("JsonData")]
        public JsonResult JsonData()
        {
            var garbageTypes = _context.GarbageType.Include(m => m.Materials).ToList();
            List<object> matgarbageType = new List<object>();
            matgarbageType.Add(new[] { "Тип Сміття", "Кількість матеріалів" });
            foreach(var g in garbageTypes)
            {
                matgarbageType.Add(new object[] { g.Name, g.Materials.Count() });
            }
            return new JsonResult(matgarbageType);
        }
    }
}