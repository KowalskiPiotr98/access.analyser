using access.analyser.Controllers;
using access.analyser.Data;
using access.analyser.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class LogControllerTests
    {
        private const string mockUserId = "0";
        private readonly Uri mockS3Url = new Uri ("http://example.com");
        private readonly string [] mockLogId = { "asdfghjkl", "oasihfoaifhosai", "afhisasf8hfah80" };
        private readonly string [] mockResources = { "/", "/index.html", "/test/subtest" };
        private readonly string [] mockUserAgents = { "Windows NT 10.0", "Mozilla 5.0 Windows NT 10.0", "Mozilla 5.0 Windows NT 6.3", "Mozilla 5.0" };
        private readonly string [] mockIps = { "1.1.1.1", "2.2.2.2", "3.3.3.3" };
        private readonly DateTime mockDate = DateTime.Today;
        private void AddLogs (ApplicationDbContext context)
        {
            context.Database.EnsureCreated ();
            context.Users.Add (new Microsoft.AspNetCore.Identity.IdentityUser ()
            {
                Id = mockUserId
            });
            context.Logs.AddRange (
                new Log () { Id = mockLogId [0], UploadDate = DateTime.Today, UserId = mockUserId, LogS3Url = mockS3Url, S3ObjectKey = "" },
                new Log () { Id = mockLogId [1], UploadDate = DateTime.Today.AddDays (-1), UserId = mockUserId, LogS3Url = mockS3Url, S3ObjectKey = "" },
                new Log () { Id = mockLogId [2], UploadDate = DateTime.Today, UserId = mockUserId, LogS3Url = mockS3Url, S3ObjectKey = "" }
                );
            context.LogEntries.AddRange (new LogEntry []
            {
                new LogEntry () {ClientIp = mockIps [0], RequestTime = mockDate, Method = LogEntry.RequestType.GET, Resource = mockResources [0], ResponseCode = 200, UserAgent = mockUserAgents [0], RawEntry ="", LogId = mockLogId [0]},
            });
            context.SaveChanges ();
        }
        private LogController GetController (ApplicationDbContext context, bool isAdmin = true, string userId = mockUserId)
        {
            var claims = new List<Claim> ()
            {
                new Claim (ClaimTypes.NameIdentifier, userId)
            };
            if (isAdmin)
            {
                claims.Add (new Claim (ClaimTypes.Role, "Admin"));
            }
            var user = new ClaimsPrincipal (new ClaimsIdentity (claims.ToArray ()));
            var controller = new LogController (context, null)
            {
                ControllerContext = new ControllerContext () { HttpContext = new DefaultHttpContext () { User = user } }
            };
            return controller;
        }

        [Fact]
        public async Task LogController_Index_NoParameters ()
        {
            using var connection = new SqliteConnection ("Data Source=:memory:");
            connection.Open ();
            using var context = new ApplicationDbContext (new DbContextOptionsBuilder<ApplicationDbContext> ().UseSqlite (connection).Options);
            AddLogs (context);
            using var controller = GetController (context);

            var result = await controller.Index (null);


            Assert.IsType<ViewResult> (result);
            Assert.NotNull ((result as ViewResult).Model as IEnumerable<Log>);
            Assert.Equal (context.Logs.Count (), ((result as ViewResult).Model as IEnumerable<Log>).Count ());
        }

        [Fact]
        public async Task LogController_Index_Empty_NotAdmin ()
        {
            using var connection = new SqliteConnection ("Data Source=:memory:");
            connection.Open ();
            using var context = new ApplicationDbContext (new DbContextOptionsBuilder<ApplicationDbContext> ().UseSqlite (connection).Options);
            AddLogs (context);
            using var controller = GetController (context, false, "adshuoasdfhuoasfho");

            var result = await controller.Index (null);


            Assert.IsType<ViewResult> (result);
            Assert.NotNull ((result as ViewResult).Model as IEnumerable<Log>);
            Assert.Empty ((result as ViewResult).Model as IEnumerable<Log>);
        }

        [Theory]
        [InlineData (0, 2)]
        public async Task LogController_Index_TimeLimits (int back, int count)
        {
            using var connection = new SqliteConnection ("Data Source=:memory:");
            connection.Open ();
            using var context = new ApplicationDbContext (new DbContextOptionsBuilder<ApplicationDbContext> ().UseSqlite (connection).Options);
            AddLogs (context);
            using var controller = GetController (context);

            var result = await controller.Index (DateTime.Today.AddDays (back));

            Assert.IsType<ViewResult> (result);
            Assert.NotNull ((result as ViewResult).Model as IEnumerable<Log>);
            Assert.Equal (count, ((result as ViewResult).Model as IEnumerable<Log>).Count ());
        }
    }
}
