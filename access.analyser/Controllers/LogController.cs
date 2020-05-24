using access.analyser.Data;
using access.analyser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace access.analyser.Controllers
{
    public class LogController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IConfiguration config;

        public LogController (ApplicationDbContext context, IConfiguration config)
        {
            this.context = context;
            this.config = config;
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

        [HttpGet]
        public async Task<IActionResult> Delete (string id)
        {
            if (id is null)
            {
                return NotFound ();
            }
            var log = await context.Logs.FindAsync (id);
            if (log is null)
            {
                return NotFound ();
            }
            if (!User.IsInRole ("Admin") && User.FindFirstValue (ClaimTypes.NameIdentifier) == log.UserId)
            {
                return NotFound ();
            }
            return View (log);
        }

        [HttpPost, ActionName ("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed (string id)
        {
            if (id is null)
            {
                return NotFound ();
            }
            var log = await context.Logs.FindAsync (id);
            if (log is null)
            {
                return NotFound ();
            }
            if (!User.IsInRole ("Admin") && User.FindFirstValue (ClaimTypes.NameIdentifier) == log.UserId)
            {
                return NotFound ();
            }
            await log.DeleteS3Object (config.GetConnectionString ("S3BucketName"));
            var entries = from l in context.LogEntries where l.LogId == id select l;
            context.LogEntries.RemoveRange (entries);
            context.Logs.Remove (log);
            await context.SaveChangesAsync ();
            return RedirectToAction (nameof (Index));
        }

        [HttpGet]
        public async Task<IActionResult> Download (string id)
        {
            if (id is null)
            {
                return NotFound ();
            }
            var log = await context.Logs.FindAsync (id);
            if (log is null)
            {
                return NotFound ();
            }
            if (!User.IsInRole ("Admin") && User.FindFirstValue (ClaimTypes.NameIdentifier) != log.UserId)
            {
                return NotFound ();
            }
            var ret = await log.GetFileStream (config.GetConnectionString ("S3BucketName"));
            if (ret is null)
            {
                return NotFound ();
            }
            return ret;
        }
    }
}