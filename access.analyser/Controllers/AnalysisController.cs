using access.analyser.Data;
using access.analyser.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace access.analyser.Controllers
{
    [Authorize]
    public class AnalysisController : Controller
    {
        private readonly ApplicationDbContext context;

        public AnalysisController (ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Allows the setup of analysis
        /// </summary>
        /// <returns>Index view</returns>
        [HttpGet]
        public IActionResult Index () => View ();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyse (DateTime? dateFrom, DateTime? dateTo, LogEntry.RequestType? type, string ip, string resource, string response, string agent)
        {
            var list = context.LogEntries.Include (l => l.Log).OrderByDescending (l => l.RequestTime).Select (l => l);
            var userId = User.FindFirstValue (ClaimTypes.NameIdentifier);
            return View (await LogEntry.FilterEntries (list, User.IsInRole ("Admin"), userId, dateFrom, dateTo, type, ip, resource, response, agent));
        }
    }
}