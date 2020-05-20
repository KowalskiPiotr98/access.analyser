using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace access.analyser.Models
{
    public class LogEntry
    {
        public enum RequestType
        {
            Other,
            GET,
            HEAD,
            POST,
            PUT,
            DELETE,
            CONNECT,
            OPTIONS,
            TRACE,
            PATCH
        }

        public enum EntrySortOrder
        {
            Date,
            Type,
            Response,
            Resource,
            Agent
        }

        [Key]
        [DatabaseGenerated (DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        /// <summary>
        /// Reference to log file from which this entry was read
        /// </summary>
        [Required]
        public string LogId { get; set; }
        public Log Log { get; set; }
        /// <summary>
        /// IP from which the request was sent
        /// </summary>
        [Required]
        [Display (Name = "Client IP")]
        public string ClientIp { get; set; }
        /// <summary>
        /// Date and time of the request
        /// </summary>
        [Required]
        [Display (Name = "Request date")]
        [DataType (DataType.DateTime)]
        public DateTime RequestTime { get; set; }
        /// <summary>
        /// HTTP request method
        /// </summary>
        [Required]
        [Display (Name = "HTTP method")]
        public RequestType Method { get; set; }
        /// <summary>
        /// Resource requested, together with query string
        /// </summary>
        /// <remarks>
        /// Might sometimes be empty.
        /// Default field to put client's request in, when it's not a correct HTTP request.
        /// </remarks>
        [Required]
        public string Resource { get; set; }
        /// <summary>
        /// Response code returned by the server
        /// </summary>
        [Required]
        [Display (Name = "Response code")]
        public int ResponseCode { get; set; }
        /// <summary>
        /// User Agent string provided in the request
        /// </summary>
        [Display (Name = "User agent")]
        public string UserAgent { get; set; }
        /// <summary>
        /// Raw line from the log
        /// </summary>
        [Required]
        [Display (Name = "Raw log entry")]
        public string RawEntry { get; set; }

        internal static async Task<IQueryable<LogEntry>> FilterEntries (IQueryable<LogEntry> list, bool isAdmin, string userId, DateTime? dateFrom, DateTime? dateTo, LogEntry.RequestType? type, string ip, string resource, string response, string agent)
        {
            if (!isAdmin)
            {
                list = from l in list where l.Log.UserId == userId select l;
            }
            if (dateFrom.HasValue)
            {
                list = from l in list where l.RequestTime >= dateFrom.Value select l;
            }
            if (dateTo.HasValue)
            {
                list = from l in list where l.RequestTime <= dateTo.Value select l;
            }
            if (type.HasValue)
            {
                list = from l in list where l.Method == type.Value select l;
            }
            if (ip != null)
            {
                ip = ip.Trim ();
                var ips = ip.Split (" ");
                list = from l in list where ips.Contains (l.ClientIp) select l;
            }
            if (resource != null)
            {
                resource = resource.Trim ();
                var resources = resource.Split (" ");
                list = from l in list where resources.Contains (l.Resource) select l;
            }
            if (response != null)
            {
                response = response.Trim ();
                var responses = response.Split (" ");
                var responsesInt = new List<int> (responses.Length);
                foreach (var item in responses)
                {
                    if (Int32.TryParse (item, out int result))
                    {
                        responsesInt.Add (result);
                    }
                }
                list = from l in list where responsesInt.Contains (l.ResponseCode) select l;
            }
            if (agent != null)
            {
                agent = agent.Trim ();
                var agents = new List<string> ();
                int index = agent.IndexOf ('\"');
                var listTempAsync = list.ToListAsync ();
                while (index >= 0)
                {
                    agent = agent.Remove (0, index + 1);
                    var endIndex = agent.IndexOf ('\"');
                    agents.Add (agent.Substring (0, endIndex));
                    agent = agent.Remove (0, endIndex + 1);
                    index = agent.IndexOf ('\"');
                }
                var listTemp = from l in await listTempAsync where agents.Any (a => l.UserAgent.Contains (a)) select l;
                list = listTemp.AsQueryable ();
            }
            return list;
        }

        internal static IQueryable<LogEntry> SortEntries (IQueryable<LogEntry> list, EntrySortOrder order, bool descending)
        {
            if (descending)
            {
                switch (order)
                {
                    case EntrySortOrder.Date:
                        return from l in list orderby l.RequestTime descending select l;
                    case EntrySortOrder.Type:
                        return from l in list orderby l.Method descending select l;
                    case EntrySortOrder.Response:
                        return from l in list orderby l.ResponseCode descending select l;
                    case EntrySortOrder.Resource:
                        return from l in list orderby l.Resource descending select l;
                    case EntrySortOrder.Agent:
                        return from l in list orderby l.UserAgent descending select l;
                }
            }
            else
            {
                switch (order)
                {
                    case EntrySortOrder.Date:
                        return from l in list orderby l.RequestTime select l;
                    case EntrySortOrder.Type:
                        return from l in list orderby l.Method select l;
                    case EntrySortOrder.Response:
                        return from l in list orderby l.ResponseCode select l;
                    case EntrySortOrder.Resource:
                        return from l in list orderby l.Resource select l;
                    case EntrySortOrder.Agent:
                        return from l in list orderby l.UserAgent select l;
                }
            }
            throw new InvalidOperationException ("Action for desired SortOrder was not found.");
        }

        public static IEnumerable<IGrouping<string, LogEntry>> GetTopIPs (IEnumerable<LogEntry> list, int limit = 10) => list.GroupBy (l => l.ClientIp).OrderByDescending (l => l.Count ()).ThenBy (l => l.Key).Take (limit);
        public static IEnumerable<IGrouping<RequestType, LogEntry>> GetTopMethods (IEnumerable<LogEntry> list, int limit = 10) => list.GroupBy (l => l.Method).OrderByDescending (l => l.Count ()).ThenBy (l => l.Key).Take (limit);
        public static IEnumerable<IGrouping<int, LogEntry>> GetTopResponses (IEnumerable<LogEntry> list, int limit = 10) => list.GroupBy (l => l.ResponseCode).OrderByDescending (l => l.Count ()).ThenBy (l => l.Key).Take (limit);
        public static IEnumerable<IGrouping<string, LogEntry>> GetTopResources (IEnumerable<LogEntry> list, int limit = 10) => list.GroupBy (l => l.Resource).OrderByDescending (l => l.Count ()).ThenBy (l => l.Key).Take (limit);
        public static IEnumerable<IGrouping<string, LogEntry>> GetTopAgents (IEnumerable<LogEntry> list, int limit = 10) => list.GroupBy (l => l.UserAgent).OrderByDescending (l => l.Count ()).ThenBy (l => l.Key).Take (limit);
    }
}
