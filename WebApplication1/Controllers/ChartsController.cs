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
            var garbageTypes = _context.GarbageType.Include(m => m.Material).ToList();
            List<object> matgarbageType = new List<object>();
            matgarbageType.Add(new[] { "Тип Сміття", "Кількість матеріалів" });
            foreach (var g in garbageTypes)
            {
                matgarbageType.Add(new object[] { g.Name, g.Material.Count() });
            }
            return new JsonResult(matgarbageType);
        }
    }
}