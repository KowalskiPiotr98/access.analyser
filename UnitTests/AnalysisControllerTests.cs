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
    public class AnalysisControllerTests
    {
        private const string mockUserId = "asdfghjkl";
        private readonly Uri mockS3Url = new Uri ("http://example.com");
        private const string mockLogId = "0";
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
            context.Logs.Add (new Log ()
            {
                Id = mockLogId,
                UploadDate = DateTime.Today,
                UserId = mockUserId,
                LogS3Url = mockS3Url,
                S3ObjectKey = ""
            });
            context.LogEntries.AddRange (new LogEntry []
            {
                new LogEntry () {ClientIp = mockIps [0], RequestTime = mockDate, Method = LogEntry.RequestType.GET, Resource = mockResources [0], ResponseCode = 200, UserAgent = mockUserAgents [0], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [1], RequestTime = mockDate.AddDays (-1), Method = LogEntry.RequestType.POST, Resource = mockResources [1], ResponseCode = 300, UserAgent = mockUserAgents [1], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [2], RequestTime = mockDate.AddDays (-1), Method = LogEntry.RequestType.GET, Resource = mockResources [2], ResponseCode = 404, UserAgent = mockUserAgents [0], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [0], RequestTime = mockDate.AddDays (-1), Method = LogEntry.RequestType.POST, Resource = mockResources [1], ResponseCode = 500, UserAgent = mockUserAgents [2], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [2], RequestTime = mockDate, Method = LogEntry.RequestType.GET, Resource = mockResources [0], ResponseCode = 200, UserAgent = mockUserAgents [0], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [1], RequestTime = mockDate, Method = LogEntry.RequestType.GET, Resource = mockResources [2], ResponseCode = 200, UserAgent = mockUserAgents [0], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [1], RequestTime = mockDate, Method = LogEntry.RequestType.POST, Resource = mockResources [1], ResponseCode = 200, UserAgent = mockUserAgents [1], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [2], RequestTime = mockDate.AddDays (-1), Method = LogEntry.RequestType.GET, Resource = mockResources [0], ResponseCode = 500, UserAgent = mockUserAgents [0], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [0], RequestTime = mockDate, Method = LogEntry.RequestType.GET, Resource = mockResources [1], ResponseCode = 200, UserAgent = mockUserAgents [0], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [0], RequestTime = mockDate, Method = LogEntry.RequestType.GET, Resource = mockResources [0], ResponseCode = 200, UserAgent = mockUserAgents [1], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [2], RequestTime = mockDate.AddDays (-1), Method = LogEntry.RequestType.PUT, Resource = mockResources [0], ResponseCode = 444, UserAgent = mockUserAgents [0], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [2], RequestTime = mockDate.AddDays (-1), Method = LogEntry.RequestType.GET, Resource = mockResources [1], ResponseCode = 200, UserAgent = mockUserAgents [2], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [1], RequestTime = mockDate.AddDays (-1), Method = LogEntry.RequestType.GET, Resource = mockResources [1], ResponseCode = 200, UserAgent = mockUserAgents [3], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [1], RequestTime = mockDate.AddDays (-1), Method = LogEntry.RequestType.GET, Resource = mockResources [0], ResponseCode = 444, UserAgent = mockUserAgents [2], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [1], RequestTime = mockDate, Method = LogEntry.RequestType.PUT, Resource = mockResources [0], ResponseCode = 200, UserAgent = mockUserAgents [1], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [0], RequestTime = mockDate, Method = LogEntry.RequestType.GET, Resource = mockResources [2], ResponseCode = 200, UserAgent = mockUserAgents [1], RawEntry ="", LogId = mockLogId},
                new LogEntry () {ClientIp = mockIps [0], RequestTime = mockDate.AddDays (-1), Method = LogEntry.RequestType.DELETE, Resource = mockResources [0], ResponseCode = 444, UserAgent = mockUserAgents [0], RawEntry ="", LogId = mockLogId},
            });
            context.SaveChanges ();
        }
        private AnalysisController GetController (ApplicationDbContext context, bool isAdmin = true, string userId = mockUserId)
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
            var controller = new AnalysisController (context)
            {
                ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext () { HttpContext = new DefaultHttpContext () { User = user } }
            };
            return controller;
        }

        [Fact]
        public async Task AnalysisController_Analyse_NoParameters_ReturnsViewWithAll ()
        {
            using var connection = new SqliteConnection ("Data Source=:memory:");
            connection.Open ();
            using var context = new ApplicationDbContext (new DbContextOptionsBuilder<ApplicationDbContext> ().UseSqlite (connection).Options);
            AddLogs (context);
            using var controller = GetController (context);

            var result = await controller.Analyse (null, null, null, null, null, null, null, null, null);


            Assert.IsType<ViewResult> (result);
            Assert.NotNull ((result as ViewResult).Model as IEnumerable<LogEntry>);
            Assert.Equal (context.LogEntries.Count (), ((result as ViewResult).Model as IEnumerable<LogEntry>).Count ());
        }

        [Theory]
        [InlineData (0, null, 8)]
        [InlineData (null, -1, 9)]
        [InlineData (-2, -2, 0)]
        public async Task AnalysisController_Analyse_TimeLimits (int? from, int? to, int count)
        {
            using var connection = new SqliteConnection ("Data Source=:memory:");
            connection.Open ();
            using var context = new ApplicationDbContext (new DbContextOptionsBuilder<ApplicationDbContext> ().UseSqlite (connection).Options);
            AddLogs (context);
            using var controller = GetController (context);
            DateTime? fromDate = from.HasValue ? DateTime.Today.AddDays (from.Value) : new DateTime? ();
            DateTime? toDate = to.HasValue ? DateTime.Today.AddDays (to.Value) : new DateTime? ();

            var result = await controller.Analyse (fromDate, toDate, null, null, null, null, null, null, null);

            Assert.IsType<ViewResult> (result);
            Assert.NotNull ((result as ViewResult).Model as IEnumerable<LogEntry>);
            Assert.Equal (count, ((result as ViewResult).Model as IEnumerable<LogEntry>).Count ());
        }

        [Fact]
        public async Task AnalysisController_Analyse_UserNotAdmin_ReturnsEmpty ()
        {
            using var connection = new SqliteConnection ("Data Source=:memory:");
            connection.Open ();
            using var context = new ApplicationDbContext (new DbContextOptionsBuilder<ApplicationDbContext> ().UseSqlite (connection).Options);
            AddLogs (context);
            using var controller = GetController (context, false, mockUserId + "asfy9af9sasf9hu");

            var result = await controller.Analyse (null, null, null, null, null, null, null, null, null);


            Assert.IsType<ViewResult> (result);
            Assert.NotNull ((result as ViewResult).Model as IEnumerable<LogEntry>);
            Assert.Empty (((result as ViewResult).Model as IEnumerable<LogEntry>));
        }

        [Theory]
        [InlineData ("1.1.1.1", 6)]
        public async Task AnalysisController_Analyse_IpFilter (string ipQuery, int count)
        {
            using var connection = new SqliteConnection ("Data Source=:memory:");
            connection.Open ();
            using var context = new ApplicationDbContext (new DbContextOptionsBuilder<ApplicationDbContext> ().UseSqlite (connection).Options);
            AddLogs (context);
            using var controller = GetController (context);

            var result = await controller.Analyse (null, null, null, ipQuery, null, null, null, null, null);

            Assert.IsType<ViewResult> (result);
            Assert.NotNull ((result as ViewResult).Model as IEnumerable<LogEntry>);
            Assert.Equal (count, ((result as ViewResult).Model as IEnumerable<LogEntry>).Count ());
        }

        [Theory]
        [InlineData ("/", 8)]
        [InlineData ("/ /index.html", 14)]
        [InlineData ("/ /index.html oneextra", 14)]
        public async Task AnalysisController_Analyse_ResourceFilter (string resourceQuery, int count)
        {
            using var connection = new SqliteConnection ("Data Source=:memory:");
            connection.Open ();
            using var context = new ApplicationDbContext (new DbContextOptionsBuilder<ApplicationDbContext> ().UseSqlite (connection).Options);
            AddLogs (context);
            using var controller = GetController (context);

            var result = await controller.Analyse (null, null, null, null, resourceQuery , null, null, null, null);

            Assert.IsType<ViewResult> (result);
            Assert.NotNull ((result as ViewResult).Model as IEnumerable<LogEntry>);
            Assert.Equal (count, ((result as ViewResult).Model as IEnumerable<LogEntry>).Count ());
        }

        [Theory]
        [InlineData ("200", 10)]
        [InlineData ("200 500", 12)]
        [InlineData ("200 500 wololo", 12)]
        public async Task AnalysisController_Analyse_ResponseFilter (string responseQuery, int count)
        {
            using var connection = new SqliteConnection ("Data Source=:memory:");
            connection.Open ();
            using var context = new ApplicationDbContext (new DbContextOptionsBuilder<ApplicationDbContext> ().UseSqlite (connection).Options);
            AddLogs (context);
            using var controller = GetController (context);

            var result = await controller.Analyse (null, null, null, null, null, responseQuery, null, null, null);

            Assert.IsType<ViewResult> (result);
            Assert.NotNull ((result as ViewResult).Model as IEnumerable<LogEntry>);
            Assert.Equal (count, ((result as ViewResult).Model as IEnumerable<LogEntry>).Count ());
        }

        [Theory]
        [InlineData ("\"Windows NT 10.0\"", 13)]
        [InlineData ("\"Windows NT 6.3\" \"Windows NT 10.0\"", 16)]
        [InlineData ("\"Windows NT 10.0\" hoasfhouasf \"fshofasho\"", 13)]
        public async Task AnalysisController_Analyse_AgentFilter (string agentQuery, int count)
        {
            using var connection = new SqliteConnection ("Data Source=:memory:");
            connection.Open ();
            using var context = new ApplicationDbContext (new DbContextOptionsBuilder<ApplicationDbContext> ().UseSqlite (connection).Options);
            AddLogs (context);
            using var controller = GetController (context);

            var result = await controller.Analyse (null, null, null, null, null, null, agentQuery, null, null);

            Assert.IsType<ViewResult> (result);
            Assert.NotNull ((result as ViewResult).Model as IEnumerable<LogEntry>);
            Assert.Equal (count, ((result as ViewResult).Model as IEnumerable<LogEntry>).Count ());
        }
    }
}
