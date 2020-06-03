using access.analyser.Data;
using access.analyser.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace access.analyser.Controllers
{
    [Authorize]
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
            list = list.Include (l => l.LogEntries).OrderByDescending (l => l.UploadDate).Select (l => l);
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
            if (!User.IsInRole ("Admin") && User.FindFirstValue (ClaimTypes.NameIdentifier) != log.UserId)
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
            if (!User.IsInRole ("Admin") && User.FindFirstValue (ClaimTypes.NameIdentifier) != log.UserId)
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
            var ret = await log.GetDownloadFileStream (config.GetConnectionString ("S3BucketName"));
            if (ret is null)
            {
                return NotFound ();
            }
            return ret;
        }

        [HttpGet]
        public IActionResult Upload () => View ();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload (List<IFormFile> files)
        {
            foreach (var file in files)
            {
                if (Path.GetExtension(file.FileName) != ".log")
                {
                    continue;//jakis blad dodac?
                }

                if (file.Length > 1024*1024*5)//5MB
                {
                    continue;//jakis blad dodac?
                }
                var logCount = context.Logs.Count (l => l.UploadDate.Date == DateTime.Now && l.UserId == User.FindFirstValue (ClaimTypes.NameIdentifier)) + 1;
                var l = new Log ()
                {
                    UserId = User.FindFirstValue (ClaimTypes.NameIdentifier),
                    UploadDate = DateTime.Now,
                    S3ObjectKey = $"{User.FindFirstValue (ClaimTypes.NameIdentifier)}-{DateTime.Now.ToString ("dd.MM.yyyy")}-{logCount}.log"
                };
                context.Logs.Add (l);
                context.SaveChanges ();
                await l.UploadFile(config.GetConnectionString("S3BucketName"), file.OpenReadStream());
            }
            //TODO: w reakcji na zwrócenie z UploadFile false albo złapanie wyjątku rzuconego w tej metodzie, log powinien zostać usunięty z bazy a user dostać info o błędzie.
            return RedirectToAction (nameof (Index));
        }
    }
}