using access.analyser.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace access.analyser.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<Log> Logs { get; set; }

        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options)
            : base (options)
        {
        }
    }
}
