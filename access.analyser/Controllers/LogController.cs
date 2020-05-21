using access.analyser.Data;
using access.analyser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace access.analyser.Controllers
{
    public class LogController : Controller
    {
        private readonly ApplicationDbContext context;

        public LogController (ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? uploadedOn)
        {
            var list = from l in context.Logs select l;
            list = Log.GetAuthorisedLogs (list, User.FindFirstValue (ClaimTypes.NameIdentifier), User.IsInRole ("Admin"));
            list = list.Include (l => l.LogEntries).Select (l => l);
            if (uploadedOn.HasValue)
            {
                list = Log.SelectByDate (list, uploadedOn.Value);
                ViewData ["UploadedOn"] = uploadedOn.Value.Date.ToString ("yyyy-MM-dd");
            }
            return View (await list.ToListAsync ());
        }
    }
}